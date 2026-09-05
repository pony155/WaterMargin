using System.Text;
using System.Windows;
using Spelljammer.Simulation;

namespace Spelljammer;

public partial class MainWindow : Window
{
    private const int FrameCount = 4;
    private readonly ExpeditionSimulation simulation = new();
    private ExpeditionState expedition;
    private ulong nextSeed = 0xc0ffeeUL;

    public MainWindow()
    {
        InitializeComponent();
        expedition = simulation.Create(nextSeed);
        Viewport.FrameChanged += Viewport_FrameChanged;
        Closed += MainWindow_Closed;
        ShowFrame(0);
        UpdateExpeditionView("The chart is mostly blank. Choose a heading and make the void legible.");
    }

    private void Apply(ExpeditionCommand command)
    {
        CommandResult result = simulation.Apply(expedition, command);
        expedition = result.State;
        UpdateExpeditionView(Describe(result));
    }

    private void UpdateExpeditionView(string eventText)
    {
        SectorSnapshot sector = simulation.Inspect(expedition);
        SectorLabel.Text = Describe(sector.Kind);
        TurnLabel.Text = $"TURN {expedition.Turn}";
        FuelLabel.Text = expedition.Fuel.ToString();
        HullLabel.Text = expedition.Hull.ToString();
        SuppliesLabel.Text = expedition.Supplies.ToString();
        CargoLabel.Text = expedition.Cargo.ToString();
        SeedLabel.Text = $"CHART {expedition.Seed:x8} · SECTOR {sector.StableId:00}";
        SectorDetailLabel.Text = $"Danger {sector.Danger} · Salvage {sector.SalvageYield}";
        SectorMapLabel.Text = BuildMap();
        EventLabel.Text = expedition.Status switch
        {
            ExpeditionStatus.Returned => "Voyage complete. The recovered cargo buys another chance among the stars.",
            ExpeditionStatus.Lost => "Voyage lost. The void keeps this ship and its unfinished stories.",
            _ => eventText,
        };
    }

    private string BuildMap()
    {
        StringBuilder map = new();
        for (int y = 0; y < SectorPosition.Height; ++y)
        {
            for (int x = 0; x < SectorPosition.Width; ++x)
            {
                SectorPosition position = new(x, y);
                char marker = position == expedition.Position
                    ? '◆'
                    : position == new SectorPosition(1, 1)
                        ? '⌂'
                        : expedition.HasVisited(position) ? '·' : '?';
                map.Append(marker).Append(' ');
            }

            if (y + 1 < SectorPosition.Height)
            {
                map.AppendLine();
            }
        }

        return map.ToString();
    }

    private static string Describe(CommandResult result)
    {
        if (!result.Accepted)
        {
            return result.Rejection switch
            {
                CommandRejection.ExpeditionEnded => "This voyage has ended. Chart a new void to continue.",
                CommandRejection.SectorBoundary => "The chart ends here; choose another heading.",
                CommandRejection.InsufficientFuel => "The drive is dry. Search this sector for a fuel cache.",
                CommandRejection.NothingToSalvage => "This sector holds nothing worth recovering.",
                CommandRejection.AlreadySalvaged => "The crew has already stripped this sector clean.",
                CommandRejection.HullAlreadySound => "The hull needs no patching.",
                CommandRejection.InsufficientCargo => "Hull patches require 2 cargo.",
                CommandRejection.NotAtAnchorage => "A voyage can end only at the anchorage.",
                CommandRejection.InsufficientPrize => "Recover at least 8 cargo before ending the voyage.",
                _ => "The command was rejected.",
            };
        }

        if (result.CargoRecovered > 0)
        {
            string fuel = result.FuelRecovered > 0 ? $" and {result.FuelRecovered} fuel" : string.Empty;
            return $"Recovered {result.CargoRecovered} cargo{fuel}.";
        }

        if (result.HullDamage > 0)
        {
            return $"The crossing dealt {result.HullDamage} hull damage.";
        }

        return result.State.Status == ExpeditionStatus.Returned
            ? "Voyage complete."
            : "Command committed.";
    }

    private static string Describe(SectorKind kind) => kind switch
    {
        SectorKind.Anchorage => "Free Anchorage",
        SectorKind.OpenVoid => "Open Void",
        SectorKind.DebrisField => "Debris Field",
        SectorKind.CrystalShoals => "Crystal Shoals",
        SectorKind.AncientRuin => "Ancient Ruin",
        SectorKind.AetherStorm => "Aether Storm",
        _ => "Unknown Sector",
    };

    private void NorthButton_Click(object sender, RoutedEventArgs e) =>
        Apply(ExpeditionCommand.Travel(TravelDirection.North));

    private void EastButton_Click(object sender, RoutedEventArgs e) =>
        Apply(ExpeditionCommand.Travel(TravelDirection.East));

    private void SouthButton_Click(object sender, RoutedEventArgs e) =>
        Apply(ExpeditionCommand.Travel(TravelDirection.South));

    private void WestButton_Click(object sender, RoutedEventArgs e) =>
        Apply(ExpeditionCommand.Travel(TravelDirection.West));

    private void SalvageButton_Click(object sender, RoutedEventArgs e) => Apply(ExpeditionCommand.Salvage);

    private void RepairButton_Click(object sender, RoutedEventArgs e) => Apply(ExpeditionCommand.Repair);

    private void ReturnButton_Click(object sender, RoutedEventArgs e) => Apply(ExpeditionCommand.ReturnHome);

    private void NewVoyageButton_Click(object sender, RoutedEventArgs e)
    {
        expedition = simulation.Create(++nextSeed);
        UpdateExpeditionView("A new chart fixes the hazards and rewards for this run.");
    }

    private void Viewport_FrameChanged(object? sender, int frame) => ShowFrame(frame);

    private void ShowFrame(int frame)
    {
        FrameLabel.Text = $"DRIVE {frame + 1} / {FrameCount} · 8 FPS";
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        Viewport.TogglePlayback();
        PlayPauseButton.Content = Viewport.IsPlaying ? "Pause drive" : "Run drive";
    }

    private void StepButton_Click(object sender, RoutedEventArgs e)
    {
        Viewport.StepFrame();
        PlayPauseButton.Content = "Run drive";
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        Viewport.Restart();
        PlayPauseButton.Content = "Pause drive";
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        Viewport.FrameChanged -= Viewport_FrameChanged;
        Closed -= MainWindow_Closed;
    }
}
