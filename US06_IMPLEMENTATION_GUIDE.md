# US06 — Park a vehicle: implementation guide

Step-by-step guide for all nine tasks in US06 (*As a member, I want to park one of my
registered vehicles in an available parking space*).

**Blocked by:** US04 (registered vehicles) and US05 (vehicle types & parking spots),
which are in turn blocked by US01–US03. Per the implementation note, we build
everything that is possible today and document the rest.

---

## Status overview

| Task | What it is | Status |
|---|---|---|
| 06.7 | Hourly rate from appsettings | ✅ Fully doable |
| 06.1 | ParkVehicleViewModel | ✅ Fully doable |
| 06.6 | Owner age ≥ 18 | ✅ Logic + tests doable |
| 06.5 | Server-side ownership/availability | ✅ Logic + tests doable |
| 06.9 | Success/failure feedback | ✅ Fully doable |
| 06.8 | Create ParkingSession | ⚠️ Factory doable, persistence blocked |
| 06.4 | Selection form | ⚠️ Markup doable, data blocked |
| 06.2 | Member's unparked vehicles | ⛔ Blocked (US04/US03/US01) |
| 06.3 | Free spots + location filter | ⛔ Blocked (US05/US03) |

**Recommended order:** 06.7 → 06.1 → 06.6 + 06.5 → 06.9 → 06.8 (factory) → 06.4 (markup)
→ *wait for unblock* → 06.2 → 06.3 → 06.4 (wiring) → 06.8 (persistence).

**Branching:** one branch per task, e.g. `TASK-06.7_HourlyRate`, PR to `Dev`.
Tasks 06.5 and 06.6 share one validator — do them in a single branch and close both issues.

---

## TASK-06.7 — Read the hourly rate from appsettings

**Goal:** the price must come from configuration, never from the form or the vehicle type.

### Steps

1. Add the section to `appsettings.json`:

```json
"Pricing": {
  "HourlyRate": 20
}
```

2. Create `Configuration/PricingOptions.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.Configuration;

public class PricingOptions
{
    public const string SectionName = "Pricing";

    [Range(0.01, 10000, ErrorMessage = "HourlyRate must be greater than 0.")]
    public decimal HourlyRate { get; set; }
}
```

3. Register in `Program.cs`:

```csharp
builder.Services
    .AddOptions<PricingOptions>()
    .Bind(builder.Configuration.GetSection(PricingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

4. Inject where needed: `IOptions<PricingOptions>` (not `IOptionsSnapshot` — the rate
   must not change mid-run).

### Why `ValidateOnStart`

If the section is deleted, `HourlyRate` binds to `0` and every parking session would be
free — silently. `ValidateOnStart` turns that into a startup failure with a clear message.

### Done when

- [ ] Section exists in `appsettings.json`
- [ ] Options class with range validation
- [ ] Registered with `ValidateDataAnnotations().ValidateOnStart()`
- [ ] Injectable via `IOptions<PricingOptions>`
- [ ] Tests green (see the test guide below)
- [ ] Rate is never read from user input

### Note for the PR

On Azure the value can be overridden with the app setting `Pricing__HourlyRate`
without redeploying.

---

## TASK-06.1 — ParkVehicleViewModel

**Goal:** a form model containing *only* what the user may choose.

### Steps

Create `ViewModels/ParkVehicleViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Garage.Web.ViewModels;

public class ParkVehicleViewModel
{
    [Required(ErrorMessage = "Choose a vehicle.")]
    [Display(Name = "Vehicle")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "Choose a parking spot.")]
    [Display(Name = "Parking spot")]
    public int ParkingSpotId { get; set; }

    [Display(Name = "Filter by location")]
    [StringLength(50)]
    public string? LocationFilter { get; set; }

    // Populated by the controller (tasks 06.2 and 06.3)
    public IEnumerable<SelectListItem> Vehicles { get; set; } = [];
    public IEnumerable<SelectListItem> ParkingSpots { get; set; } = [];
}
```

### Critical design point

There is **no** `CheckInTime`, `HourlyRateAtCheckIn` or `OwnerId` property.
That absence *is* the implementation of three acceptance criteria:

- `CheckInTime` is set by the system and cannot be manipulated through the form
- `HourlyRateAtCheckIn` comes from appsettings and cannot be manipulated
- No action trusts a posted `OwnerId`

Write this explicitly in the PR description — reviewers look for it.

### Done when

- [ ] ViewModel contains only `VehicleId`, `ParkingSpotId`, `LocationFilter` + select lists
- [ ] Required validation with readable messages
- [ ] No time, price or owner properties

---

## TASK-06.6 + TASK-06.5 — Server-side validation (one validator)

**Goal:** every rule is revalidated on the server on submit, regardless of what the
form or the URL claims.

### Step 1: Result type

`Services/ParkingError.cs`:

```csharp
namespace Garage.Web.Services;

public enum ParkingError
{
    None,
    VehicleNotOwnedByUser,
    VehicleAlreadyParked,
    OwnerUnderage,
    SpotOutOfService,
    SpotOccupied
}
```

### Step 2: The facts the rules operate on

Deliberately *not* EF entities — this keeps the logic compilable and testable while
US03–US05 are unfinished, and makes it independent of their final entity shape.

```csharp
public record ParkingRequest(
    int VehicleId,
    string VehicleOwnerId,
    DateOnly OwnerDateOfBirth,
    bool VehicleHasActiveSession,
    int SpotId,
    bool SpotIsOutOfService,
    bool SpotHasActiveSession);
```

### Step 3: The validator

`Services/ParkingRules.cs`:

```csharp
namespace Garage.Web.Services;

public static class ParkingRules
{
    public const int MinimumOwnerAge = 18;

    public static ParkingError Validate(ParkingRequest r, string currentUserId, DateTime now)
    {
        if (r.VehicleOwnerId != currentUserId)          return ParkingError.VehicleNotOwnedByUser;
        if (r.VehicleHasActiveSession)                  return ParkingError.VehicleAlreadyParked;
        if (AgeAt(r.OwnerDateOfBirth, now) < MinimumOwnerAge) return ParkingError.OwnerUnderage;
        if (r.SpotIsOutOfService)                       return ParkingError.SpotOutOfService;
        if (r.SpotHasActiveSession)                     return ParkingError.SpotOccupied;

        return ParkingError.None;
    }

    public static int AgeAt(DateOnly birthDate, DateTime at)
    {
        var today = DateOnly.FromDateTime(at);
        var age = today.Year - birthDate.Year;
        if (today < birthDate.AddYears(age)) age--;
        return age;
    }
}
```

**Order matters:** ownership is checked first so an attacker probing ids learns nothing
about other members' vehicles.

### Step 4: Tests (all six scenarios from the user story)

```csharp
public class ParkingRulesTests
{
    private const string Me = "user-1";
    private const string SomeoneElse = "user-2";
    private static readonly DateTime Now = new(2026, 08, 06, 12, 00, 00);
    private static readonly DateOnly Adult = new(1990, 1, 1);

    private static ParkingRequest Request(
        string ownerId = Me, DateOnly? dob = null,
        bool vehicleParked = false, bool outOfService = false, bool spotTaken = false)
        => new(1, ownerId, dob ?? Adult, vehicleParked, 10, outOfService, spotTaken);

    [Fact] // Scenario 1
    public void OwnVehicle_AvailableSpot_IsAccepted()
        => Assert.Equal(ParkingError.None, ParkingRules.Validate(Request(), Me, Now));

    [Fact] // Scenario 2
    public void VehicleWithActiveSession_IsRejected()
        => Assert.Equal(ParkingError.VehicleAlreadyParked,
                        ParkingRules.Validate(Request(vehicleParked: true), Me, Now));

    [Fact] // Scenario 3
    public void OccupiedSpot_IsRejected()
        => Assert.Equal(ParkingError.SpotOccupied,
                        ParkingRules.Validate(Request(spotTaken: true), Me, Now));

    [Fact] // Scenario 4
    public void OutOfServiceSpot_IsRejected()
        => Assert.Equal(ParkingError.SpotOutOfService,
                        ParkingRules.Validate(Request(outOfService: true), Me, Now));

    [Fact] // Scenario 5
    public void OtherMembersVehicle_IsRejected()
        => Assert.Equal(ParkingError.VehicleNotOwnedByUser,
                        ParkingRules.Validate(Request(ownerId: SomeoneElse), Me, Now));

    [Fact] // Scenario 6
    public void UnderageOwner_IsRejected()
        => Assert.Equal(ParkingError.OwnerUnderage,
                        ParkingRules.Validate(Request(dob: new DateOnly(2010, 1, 1)), Me, Now));

    // Age boundaries — where this kind of check usually breaks
    [Theory]
    [InlineData(2008, 08, 06, ParkingError.None)]          // turns 18 exactly today
    [InlineData(2008, 08, 07, ParkingError.OwnerUnderage)] // 18 tomorrow
    [InlineData(2008, 02, 29, ParkingError.None)]          // leap-year birthday
    public void AgeBoundaries(int y, int m, int d, ParkingError expected)
        => Assert.Equal(expected,
                        ParkingRules.Validate(Request(dob: new DateOnly(y, m, d)), Me, Now));

    [Fact]
    public void OwnershipIsCheckedBeforeEverythingElse()
        => Assert.Equal(ParkingError.VehicleNotOwnedByUser,
                        ParkingRules.Validate(
                            Request(ownerId: SomeoneElse, vehicleParked: true, spotTaken: true),
                            Me, Now));
}
```

### Done when

- [ ] All five rules implemented in one place
- [ ] Ownership checked first
- [ ] `currentUserId` comes from the server, never from the form
- [ ] All six user-story scenarios covered by tests
- [ ] Age boundary cases tested (today, tomorrow, leap year)

### Dependency to raise now

`ApplicationUser` must have a `DateOfBirth` property. US01/US03 do not mention it —
without it, task 06.6 cannot be completed. Ask on issue #26 **before** their migration
is created.

---

## TASK-06.9 — Clear feedback

**Goal:** the user always knows whether the parking succeeded, and why it failed.

### Step 1: Two channels, chosen deliberately

- **Success** → redirect to overview → message must survive a redirect → **TempData**
- **Failure** → redisplay the form with input intact → **ModelState** + validation summary

Using TempData for failures loses the user's input or shows the message on the wrong page.

### Step 2: Error → message mapping

`Services/ParkingErrorMessages.cs`:

```csharp
public static class ParkingErrorMessages
{
    public static string ToUserMessage(this ParkingError error) => error switch
    {
        ParkingError.VehicleNotOwnedByUser => "You can only park your own vehicles.",
        ParkingError.VehicleAlreadyParked  => "This vehicle already has an active parking session.",
        ParkingError.OwnerUnderage         => "The vehicle owner must be at least 18 years old to park.",
        ParkingError.SpotOutOfService      => "That parking spot is out of service. Please choose another.",
        ParkingError.SpotOccupied          => "That parking spot was just taken. Please choose another.",
        ParkingError.None                  => string.Empty,
        _                                  => "The vehicle could not be parked. Please try again."
    };
}
```

The ownership message deliberately does not reveal whether the vehicle exists.

### Step 3: Shared alert partial

`Views/Shared/_Alert.cshtml`:

```html
@{
    var success = TempData["SuccessMessage"] as string;
    var error = TempData["ErrorMessage"] as string;
}

@if (!string.IsNullOrEmpty(success))
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        @success
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}

@if (!string.IsNullOrEmpty(error))
{
    <div class="alert alert-danger alert-dismissible fade show" role="alert">
        @error
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}
```

Render once in `_Layout.cshtml` above `@RenderBody()` — then every feature in the app
gets consistent feedback, including US04 and US05 later.

### Step 4: Tests

```csharp
[Fact]
public void EveryErrorValue_HasAMessage()
{
    foreach (var e in Enum.GetValues<ParkingError>().Where(e => e != ParkingError.None))
        Assert.False(string.IsNullOrWhiteSpace(e.ToUserMessage()));
}
```

This fails automatically if someone adds a new error case and forgets its message.

### Done when

- [ ] Every `ParkingError` maps to a clear message
- [ ] Success = TempData + redirect, failure = ModelState + redisplay
- [ ] Alert partial rendered from the layout
- [ ] `role="alert"` for screen readers
- [ ] Success message names the vehicle and the spot number

---

## TASK-06.8 — Create the ParkingSession

**Goal:** `CheckInTime` and `HourlyRateAtCheckIn` are stamped by the server, and no
incomplete session is ever persisted.

### Doable now: the factory

```csharp
public record NewParkingSession(
    int VehicleId, int ParkingSpotId, DateTime CheckInTime, decimal HourlyRateAtCheckIn);

public static class ParkingSessionFactory
{
    public static NewParkingSession Create(int vehicleId, int spotId, DateTime now, decimal hourlyRate)
        => new(vehicleId, spotId, now, hourlyRate);
}
```

Tests: assert `CheckInTime` comes from the passed clock (not from any input),
and that the rate equals `PricingOptions.HourlyRate`.

### Blocked: persistence

When US03's `ParkingSession` entity exists:

```csharp
await using var tx = await _context.Database.BeginTransactionAsync();

// re-validate INSIDE the transaction — state may have changed since the form was rendered
var error = ParkingRules.Validate(await BuildRequestAsync(vm), currentUserId, DateTime.Now);
if (error != ParkingError.None) return Fail(error);

_context.ParkingSessions.Add(new ParkingSession { ... });
await _context.SaveChangesAsync();
await tx.CommitAsync();
```

`CheckOutTime` and `TotalPrice` stay `null` while the session is active (US03).

### Dependency to raise now

Ask US03 for a **filtered unique index** so the database itself prevents two active
sessions on one spot:

```csharp
modelBuilder.Entity<ParkingSession>()
    .HasIndex(s => s.ParkingSpotId)
    .IsUnique()
    .HasFilter("[CheckOutTime] IS NULL");
```

Application checks alone lose to a race between two simultaneous submissions.

### Done when

- [ ] Factory stamps time and rate server-side (tested)
- [ ] *(after unblock)* Save wrapped in a transaction with re-validation
- [ ] *(after unblock)* Nothing persisted when validation fails
- [ ] Filtered unique index requested from US03

---

## TASK-06.4 — The selection form

**Goal:** a form for choosing a vehicle and an available spot, with a location filter.

### Doable now: the markup

`Views/Parking/Park.cshtml`:

```html
@model Garage.Web.ViewModels.ParkVehicleViewModel

<h1>Park a vehicle</h1>

<form asp-action="Park" method="get" class="mb-3">
    <div class="input-group" style="max-width:400px;">
        <input asp-for="LocationFilter" class="form-control" placeholder="Filter spots by location" />
        <button type="submit" class="btn btn-outline-primary">Filter</button>
    </div>
</form>

<form asp-action="Park" method="post">
    <div asp-validation-summary="All" class="text-danger mb-3"></div>

    <div class="form-group mb-3">
        <label asp-for="VehicleId"></label>
        <select asp-for="VehicleId" asp-items="Model.Vehicles" class="form-control">
            <option value="">-- Choose vehicle --</option>
        </select>
        <span asp-validation-for="VehicleId" class="text-danger"></span>
    </div>

    <div class="form-group mb-3">
        <label asp-for="ParkingSpotId"></label>
        <select asp-for="ParkingSpotId" asp-items="Model.ParkingSpots" class="form-control">
            <option value="">-- Choose spot --</option>
        </select>
        <span asp-validation-for="ParkingSpotId" class="text-danger"></span>
    </div>

    <input type="hidden" asp-for="LocationFilter" />

    <button type="submit" class="btn btn-success">Park</button>
    <a asp-action="Index" class="btn btn-secondary">Cancel</a>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

Note: the filter is a separate **GET** form (bookmarkable, doesn't submit the parking),
and the hidden `LocationFilter` keeps the filter if the POST fails and the form redisplays.

### Blocked

The dropdowns stay empty until 06.2 and 06.3 provide the data.

### Done when

- [ ] Form renders with both dropdowns and the location filter
- [ ] Validation summary and per-field messages present
- [ ] Empty-option placeholders so nothing is preselected
- [ ] *(after unblock)* Dropdowns populated and repopulated on every error path

---

## TASK-06.2 — The member's unparked vehicles ⛔ BLOCKED

**Needs:** `Vehicle` entity with `OwnerId` (US04), `ParkingSession` (US03),
current user id (US01).

**Reference implementation for when it unblocks:**

```csharp
var userId = _userManager.GetUserId(User);

var vehicles = await _context.Vehicles
    .Where(v => v.OwnerId == userId)
    .Where(v => !v.ParkingSessions.Any(s => s.CheckOutTime == null))
    .OrderBy(v => v.RegistrationNumber)
    .Select(v => new SelectListItem
    {
        Value = v.Id.ToString(),
        Text = $"{v.RegistrationNumber} — {v.VehicleType.Name}"
    })
    .ToListAsync();
```

Both `Where` clauses are security-relevant: the first is the ownership boundary, the
second prevents double parking. Neither may be replaced by client-side filtering.

---

## TASK-06.3 — Available spots + location filter ⛔ BLOCKED

**Needs:** `ParkingSpot` with `Location` and `IsOutOfService` (US05),
`ParkingSession` (US03).

**Reference implementation:**

```csharp
var spots = await _context.ParkingSpots
    .Where(s => !s.IsOutOfService)
    .Where(s => !s.ParkingSessions.Any(ps => ps.CheckOutTime == null))
    .Where(s => location == null || s.Location.Contains(location))
    .OrderBy(s => s.SpotNumber)
    .Select(s => new SelectListItem
    {
        Value = s.Id.ToString(),
        Text = $"{s.SpotNumber} ({s.Location})"
    })
    .ToListAsync();
```

Occupancy is **computed** from active sessions — never stored as an `IsOccupied`
column (US03 forbids it explicitly).

---

## Appendix A — What we cannot complete, and why

For the issue comment / PR description:

| Blocked item | Requires | From |
|---|---|---|
| List only the member's own vehicles | `Vehicle.OwnerId`, current user | US04, US01 |
| Vehicle dropdown with type name | `Vehicle`, `VehicleType` | US03, US04 |
| Filter/search spots by location | `ParkingSpot.Location` | US05 |
| Exclude out-of-service spots | `ParkingSpot.IsOutOfService` | US05 |
| Occupancy from active session | `ParkingSession` | US03 |
| Persist session atomically | `ParkingSession` + migration | US03 |
| Age check against real data | `ApplicationUser.DateOfBirth` | US01/US03 — **not yet specified** |

---

## Appendix B — Requests to the team (raise before US03's migration exists)

1. **`DateOfBirth` on `ApplicationUser`** — US06 requires an age check; no blocking
   story mentions storing a birth date. Without it, task 06.6 is impossible.
2. **Hourly rate: flat or per vehicle type?** US05 says `VehicleType` has no price and
   US03 puts the rate in appsettings — but one rate for both a motorcycle and a bus is
   questionable. If it should vary, the appsettings shape must be a dictionary.
3. **Filtered unique index on active sessions per spot** — application-level checks
   cannot prevent a race between two simultaneous submissions.
