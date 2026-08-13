using Garage.Web.Configuration;
using Garage.Web.Constants;
using Garage.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace Garage.Web.Data
{
	public static class DbInitializer
	{
		// Development-only default password for all seeded accounts.
		// Must never be used for self-registered accounts (see "Registrera och logga in" AC).
		private const string DevPassword = "Dev@Password123";

		private sealed record SeedUserDefinition(
			string UserName,
			string FirstName,
			string LastName,
			string PersonalNumber,
			DateTime ProMembershipStart,
			DateTime ProMembershipEnd,
			string RoleName);

		private sealed record SeedVehicleDefinition(
			string RegistrationNumber,
			string Color,
			string Model,
			int NumberOfWheels,
			string VehicleTypeName,
			string BrandName,
			string OwnerUserName);

		// Duration == null means an active session (no check-out yet).
		// SpotNumbers is only used for active sessions - historical ones were never
		// allocated a spot in the original seed data.
		private sealed record SeedSessionDefinition(
			string RegistrationNumber,
			TimeSpan CheckInAgo,
			TimeSpan? Duration,
			string[] SpotNumbers);

		private static readonly DateTime Now = DateTime.Now;

		// One vehicle owner per line, except devuser who owns two (a car and a motorcycle).
		// ProMembership periods are deliberately varied to exercise different scenarios:
		// a brand new 30-day membership, an already-expired one, and a 65+ two-year one.
		private static readonly List<SeedUserDefinition> Users = new()
		{
			new("devuser",   "Dev",     "User",      "19900101-0001", Now,                Now.AddDays(30),  RoleNames.Member),
			new("devuser1",  "Devone",  "Andersson", "19900101-0003", Now.AddDays(-60),   Now.AddDays(-30), RoleNames.Member), // expired Pro
            new("devuser2",  "Devtwo",  "Bergman",   "19900101-0004", Now,                Now.AddYears(2),  RoleNames.Member), // 65+ rule
            new("devuser3",  "Devthree","Carlsson",  "19900101-0005", Now,                Now.AddDays(30),  RoleNames.Member),
			new("devuser4",  "Devfour", "Dahlgren",  "19900101-0006", Now.AddDays(-90),   Now.AddDays(-60), RoleNames.Member), // expired long ago
            new("devuser5",  "Devfive", "Ekstrom",   "19900101-0007", Now,                Now.AddYears(2),  RoleNames.Member), // 65+ rule
            new("devuser6",  "Devsix",  "Forsberg",  "19900101-0008", Now,                Now.AddDays(30),  RoleNames.Member),
			new("devuser7",  "Devseven","Gustafsson","19900101-00０９", Now.AddDays(-25), Now.AddDays(5),   RoleNames.Member), // Pro ending soon
            new("adminuser", "Admin",   "User",      "199００１０１-０００２", Now,         Now.AddYears(2),  RoleNames.Admin),
		};

		private static readonly List<SeedVehicleDefinition> Vehicles = new()
		{
			new("ABC123", "Red",    "Corolla",  4, "Car",        "Toyota",      "devuser"),
			new("MC-001", "Black",  "CB500",    2, "Motorcycle", "Honda",       "devuser"),
			new("DEF456", "Blue",   "Civic",    4, "Car",        "Honda",       "devuser1"),
			new("GHI789", "Black",  "Focus",    4, "Car",        "Ford",        "devuser2"),
			new("JKL012", "White",  "Golf",     4, "Car",        "Volkswagen",  "devuser3"),
			new("MNO345", "Silver", "X5",       4, "Car",        "BMW",         "devuser4"),
			new("PQR678", "Yellow", "Sprinter", 6, "Bus",        "MercedesBenz","devuser5"),
			new("YZA567", "Gray",   "Elantra",  4, "Car",        "Hyundai",     "devuser6"),
			new("BCD890", "Orange", "Sportage", 4, "Car",        "Kia",         "devuser7"),
		};

		// Check-in times are relative to "now at seed time" rather than fixed dates,
		// so the data still looks recent no matter when Seed() runs.
		private static readonly List<SeedSessionDefinition> Sessions = new()
		{
            // Active sessions - varied check-in times, no CheckOutTime/TotalPrice yet.
            new("ABC123", TimeSpan.FromHours(2),                              null, new[] { "CAR-01" }),
			new("MC-001", TimeSpan.FromMinutes(45),                           null, new[] { "MC-01" }),
			new("GHI789", TimeSpan.FromHours(5),                              null, new[] { "CAR-02" }),
			new("MNO345", TimeSpan.FromMinutes(20),                           null, new[] { "CAR-03" }),
			new("BCD890", TimeSpan.FromHours(1),                              null, new[] { "CAR-04" }),
            // A bus needs two spots - good example data for the ParkingAllocation relationship.
            new("PQR678", TimeSpan.FromHours(3),                              null, new[] { "BUS-01", "BUS-02" }),
 
            // Historical, checked-out sessions - varied durations, to test receipts/history/revenue.
            new("DEF456", TimeSpan.FromDays(1) + TimeSpan.FromHours(5), TimeSpan.FromHours(3), Array.Empty<string>()),
			new("JKL012", TimeSpan.FromDays(2) + TimeSpan.FromHours(4), TimeSpan.FromHours(3), Array.Empty<string>()),
			new("YZA567", TimeSpan.FromDays(7) + TimeSpan.FromHours(6), TimeSpan.FromHours(3), Array.Empty<string>()),
		};

		public static void Seed(AppDbContext context, PricingOptions pricingOptions)
		{
			SeedVehicleTypes(context);
			SeedBrandTypes(context);
			SeedParkingSpots(context);
			SeedRoles(context);

			var users = SeedUsers(context);
			SeedUserRoles(context, users);

			SeedVehicles(context, users);
			SeedParkingSessions(context, pricingOptions);
		}

		private static void SeedVehicleTypes(AppDbContext context)
		{
			var existingNames = context.VehicleTypes.Select(t => t.Name).ToHashSet();
			var names = new[] { "Car", "Bus", "Motorcycle" };

			foreach (var name in names)
			{
				if (!existingNames.Contains(name))
					context.VehicleTypes.Add(new VehicleType { Name = name });
			}

			context.SaveChanges();
		}

		private static void SeedBrandTypes(AppDbContext context)
		{
			var existingNames = context.BrandTypes.Select(b => b.Name).ToHashSet();
			var names = new[]
			{
				"Toyota", "Honda", "Ford", "Volkswagen", "BMW",
				"MercedesBenz", "Nissan", "Hyundai", "Kia"
			};

			foreach (var name in names)
			{
				if (!existingNames.Contains(name))
					context.BrandTypes.Add(new BrandType { Name = name });
			}

			context.SaveChanges();
		}

		private static void SeedParkingSpots(AppDbContext context)
		{
			// Total spot count is fixed once seeded - admins only reassign a spot's
			// VehicleTypeId later, they never add or remove rows (see AC:
			// "Admin ska inte kunna ändra max antal garageplatser").
			if (context.ParkingSpots.Any())
				return;

			var carType = context.VehicleTypes.Single(t => t.Name == "Car");
			var busType = context.VehicleTypes.Single(t => t.Name == "Bus");
			var motorcycleType = context.VehicleTypes.Single(t => t.Name == "Motorcycle");

			var spots = new List<ParkingSpot>();

			for (int i = 1; i <= 2; i++)
				spots.Add(new ParkingSpot { SpotNumber = $"MC-{i:00}", Location = "Ground floor", VehicleTypeId = motorcycleType.Id });

			for (int i = 1; i <= 4; i++)
				spots.Add(new ParkingSpot { SpotNumber = $"BUS-{i:00}", Location = "Ground floor", VehicleTypeId = busType.Id });

			for (int i = 1; i <= 14; i++)
				spots.Add(new ParkingSpot { SpotNumber = $"CAR-{i:00}", Location = "Level 1", VehicleTypeId = carType.Id });

			context.ParkingSpots.AddRange(spots);
			context.SaveChanges();
		}

		private static void SeedRoles(AppDbContext context)
		{
			var existingNames = context.Roles.Select(r => r.Name).ToHashSet();

			foreach (var name in RoleNames.All)
			{
				if (!existingNames.Contains(name))
				{
					context.Roles.Add(new IdentityRole
					{
						Id = Guid.NewGuid().ToString(),
						Name = name,
						NormalizedName = name.ToUpperInvariant()
					});
				}
			}

			context.SaveChanges();
		}

		private static Dictionary<string, ApplicationUser> SeedUsers(AppDbContext context)
		{
			var wantedNames = Users.Select(def => def.UserName).ToList();

			var existing = context.Users
				.Where(u => wantedNames.Contains(u.UserName!))
				.ToDictionary(u => u.UserName!);

			var hasher = new PasswordHasher<ApplicationUser>();
			var toAdd = new List<ApplicationUser>();

			foreach (var def in Users)
			{
				if (existing.ContainsKey(def.UserName))
					continue;

				var user = new ApplicationUser
				{
					UserName = def.UserName,
					NormalizedUserName = def.UserName.ToUpperInvariant(),
					Email = $"{def.UserName}@example.com",
					NormalizedEmail = $"{def.UserName}@example.com".ToUpperInvariant(),
					EmailConfirmed = true,
					FirstName = def.FirstName,
					LastName = def.LastName,
					PersonalNumber = def.PersonalNumber,
					ProMembershipStart = def.ProMembershipStart,
					ProMembershipEnd = def.ProMembershipEnd,
					SecurityStamp = Guid.NewGuid().ToString("D")
				};

				user.PasswordHash = hasher.HashPassword(user, DevPassword);
				toAdd.Add(user);
			}

			if (toAdd.Count > 0)
			{
				context.Users.AddRange(toAdd);
				context.SaveChanges();
			}

			foreach (var user in toAdd)
				existing[user.UserName!] = user;

			return existing;
		}

		private static void SeedUserRoles(AppDbContext context, Dictionary<string, ApplicationUser> users)
		{
			var roles = context.Roles.ToDictionary(r => r.Name!);
			int added = 0;

			foreach (var def in Users)
			{
				var user = users[def.UserName];
				var role = roles[def.RoleName];

				bool alreadyAssigned = context.UserRoles
					.Any(ur => ur.UserId == user.Id && ur.RoleId == role.Id);

				if (!alreadyAssigned)
				{
					context.UserRoles.Add(new IdentityUserRole<string>
					{
						UserId = user.Id,
						RoleId = role.Id
					});
					added++;
				}
			}

			if (added > 0)
				context.SaveChanges();
		}

		private static void SeedVehicles(AppDbContext context, Dictionary<string, ApplicationUser> users)
		{
			var existingRegs = context.Vehicles.Select(v => v.RegistrationNumber).ToHashSet();

			var vehicleTypes = context.VehicleTypes.ToDictionary(t => t.Name!);
			var brandTypes = context.BrandTypes.ToDictionary(b => b.Name!);

			var toAdd = Vehicles
				.Where(def => !existingRegs.Contains(def.RegistrationNumber))
				.Select(def => new Vehicle
				{
					RegistrationNumber = def.RegistrationNumber,
					Color = def.Color,
					Model = def.Model,
					NumberOfWheels = def.NumberOfWheels,
					VehicleTypeId = vehicleTypes[def.VehicleTypeName].Id,
					BrandTypeId = brandTypes[def.BrandName].Id,
					OwnerId = users[def.OwnerUserName].Id
				})
				.ToList();

			if (toAdd.Count > 0)
			{
				context.Vehicles.AddRange(toAdd);
				context.SaveChanges();
			}
		}

		private static void SeedParkingSessions(AppDbContext context, PricingOptions pricingOptions)
		{
			var vehiclesByReg = context.Vehicles.ToDictionary(v => v.RegistrationNumber);
			var spotsByNumber = context.ParkingSpots.ToDictionary(s => s.SpotNumber);

			// Vehicle type name per registration number, taken from the seed definitions
			// themselves rather than re-querying VehicleTypes - avoids an extra join just
			// to find out what PricingOptions.RateFor(...) should be called with.
			var vehicleTypeNameByReg = Vehicles.ToDictionary(v => v.RegistrationNumber, v => v.VehicleTypeName);

			// A vehicle only ever gets ONE seeded session - used to decide what's
			// already there, since CheckInTime is relative to "now" and therefore
			// never matches exactly between two separate runs of Seed().
			var vehicleIdsWithSession = context.ParkingSessions
				.Select(ps => ps.VehicleId)
				.ToHashSet();

			var now = DateTime.Now;

			foreach (var def in Sessions)
			{
				var vehicle = vehiclesByReg[def.RegistrationNumber];

				if (vehicleIdsWithSession.Contains(vehicle.Id))
					continue;

				var hourlyRate = pricingOptions.RateFor(vehicleTypeNameByReg[def.RegistrationNumber]);
				var checkInTime = now - def.CheckInAgo;

				if (def.Duration == null)
				{
					var spots = def.SpotNumbers.Select(n => spotsByNumber[n]).ToArray();
					AddActiveSession(context, vehicle, checkInTime, hourlyRate, spots);
				}
				else
				{
					var checkOutTime = checkInTime + def.Duration.Value;
					AddHistoricalSession(context, vehicle, checkInTime, checkOutTime, hourlyRate, pricingOptions);
				}
			}
		}

		private static void AddActiveSession(AppDbContext context, Vehicle vehicle, DateTime checkInTime, decimal hourlyRate, params ParkingSpot[] spots)
		{
			var session = new ParkingSession
			{
				VehicleId = vehicle.Id,
				CheckInTime = checkInTime,
				HourlyRateAtCheckIn = hourlyRate
			};

			context.ParkingSessions.Add(session);
			context.SaveChanges(); // needed to get session.Id for the allocations below

			foreach (var spot in spots)
			{
				context.ParkingAllocations.Add(new ParkingAllocation
				{
					ParkingSessionId = session.Id,
					ParkingSpotId = spot.Id
				});
			}

			context.SaveChanges();
		}

		private static void AddHistoricalSession(AppDbContext context, Vehicle vehicle, DateTime checkInTime, DateTime checkOutTime, decimal hourlyRate, PricingOptions pricingOptions)
		{
			context.ParkingSessions.Add(new ParkingSession
			{
				VehicleId = vehicle.Id,
				CheckInTime = checkInTime,
				CheckOutTime = checkOutTime,
				HourlyRateAtCheckIn = hourlyRate,
				// Same "started hour" billing rule the real checkout will use -
				// keeps seed data consistent with actual application behavior.
				TotalPrice = pricingOptions.CalculatePrice(checkOutTime - checkInTime, hourlyRate)
			});

			context.SaveChanges();
		}
	}
}