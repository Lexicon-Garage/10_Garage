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
