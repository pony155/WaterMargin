using Spelljammer.Simulation;

return SimulationContracts.Run();

internal static class SimulationContracts
{
    public static int Run()
    {
        EquivalentSeedsProduceEquivalentRuns();
        RejectedCommandsDoNotAdvanceState();
        SalvageIsFiniteAndRepairHasARealCost();
        Console.WriteLine("Space expedition simulation contracts passed.");
        return 0;
    }

    private static void EquivalentSeedsProduceEquivalentRuns()
    {
        ExpeditionSimulation simulation = new();
        ExpeditionCommand[] commands =
        [
            ExpeditionCommand.Travel(TravelDirection.North),
            ExpeditionCommand.Salvage,
            ExpeditionCommand.Travel(TravelDirection.East),
            ExpeditionCommand.Salvage,
        ];

        ExpeditionState first = simulation.Create(0x5eedUL);
        ExpeditionState second = simulation.Create(0x5eedUL);
        foreach (ExpeditionCommand command in commands)
        {
            first = simulation.Apply(first, command).State;
            second = simulation.Apply(second, command).State;
        }

        Equal(first.Turn, second.Turn, "Turn count diverged for an identical seed and command stream.");
        Equal(first.Position, second.Position, "Position diverged for an identical seed and command stream.");
        Equal(first.Fuel, second.Fuel, "Fuel diverged for an identical seed and command stream.");
        Equal(first.Hull, second.Hull, "Hull diverged for an identical seed and command stream.");
        Equal(first.Cargo, second.Cargo, "Cargo diverged for an identical seed and command stream.");
    }

    private static void RejectedCommandsDoNotAdvanceState()
    {
        ExpeditionSimulation simulation = new();
        ExpeditionState state = simulation.Create(7);
        CommandResult result = simulation.Apply(state, ExpeditionCommand.ReturnHome);

        False(result.Accepted, "An empty expedition was allowed to return with a prize.");
        Equal(CommandRejection.InsufficientPrize, result.Rejection, "The rejection reason was not stable.");
        True(ReferenceEquals(state, result.State), "A rejected command replaced authoritative state.");
    }

    private static void SalvageIsFiniteAndRepairHasARealCost()
    {
        ExpeditionSimulation simulation = new();
        ExpeditionState state = simulation.Create(11);
        state = simulation.Apply(state, ExpeditionCommand.Travel(TravelDirection.North)).State;

        CommandResult firstSalvage = simulation.Apply(state, ExpeditionCommand.Salvage);
        True(firstSalvage.Accepted, "A fresh sector could not be salvaged.");
        CommandResult repeatedSalvage = simulation.Apply(firstSalvage.State, ExpeditionCommand.Salvage);
        False(repeatedSalvage.Accepted, "A sector yielded unbounded salvage.");
        Equal(CommandRejection.AlreadySalvaged, repeatedSalvage.Rejection, "Repeat salvage reason was wrong.");

        if (firstSalvage.State.Hull < ExpeditionSimulation.MaximumHull &&
            firstSalvage.State.Cargo >= ExpeditionSimulation.RepairCargoCost)
        {
            CommandResult repair = simulation.Apply(firstSalvage.State, ExpeditionCommand.Repair);
            True(repair.Accepted, "A funded hull repair was rejected.");
            Equal(
                firstSalvage.State.Cargo - ExpeditionSimulation.RepairCargoCost,
                repair.State.Cargo,
                "Repair did not consume its documented cargo cost.");
        }
    }

    private static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void False(bool condition, string message) => True(!condition, message);

    private static void Equal<T>(T expected, T actual, string message)
        where T : notnull
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message} Expected '{expected}', got '{actual}'.");
        }
    }
}
