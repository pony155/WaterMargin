using System.Collections.Immutable;
using Spelljammer.Simulation.Content;

namespace Spelljammer.Simulation.Characters;

public sealed record TrainingCompletionEvent(
    CharacterId CharacterId,
    TrainingProjectId ProjectId,
    ImmutableArray<FeatId> GrantedFeatIds,
    ImmutableArray<AccessId> GrantedAccessIds);

public sealed record TrainingContributionResult(
    CharacterState State,
    bool Accepted,
    string RejectionCode,
    int Progress,
    TrainingCompletionEvent? Completion);

public static class CharacterTrainingSystem
{
    public static TrainingContributionResult Contribute(
        CharacterState character,
        TrainingProjectId projectId,
        int workUnits,
        ICharacterContentCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(catalog);
        if (character.ContentFingerprint != catalog.Fingerprint)
        {
            return Rejected(character, ActionRejectionCodes.ContentMismatch);
        }

        if (!catalog.TryGetTrainingProject(projectId, out TrainingProjectDefinition? project))
        {
            return Rejected(character, ActionRejectionCodes.ActionUnknown);
        }

        if (workUnits <= 0 || workUnits > project!.WorkUnits)
        {
            return Rejected(character, "command.training-work-invalid");
        }

        foreach (SkillId skillId in project.RequiredSkillIds)
        {
            if (!character.Capabilities.TryGetSkill(skillId, catalog, out byte value, out _) || value == 0)
            {
                return Rejected(character, ActionRejectionCodes.SkillRequired);
            }
        }

        ImmutableArray<FeatDefinition>.Builder feats = ImmutableArray.CreateBuilder<FeatDefinition>();
        foreach (FeatId featId in project.GrantedFeatIds)
        {
            if (!catalog.TryGetFeat(featId, out FeatDefinition? feat) || feat!.TrainingProjectId != projectId)
            {
                return Rejected(character, ActionRejectionCodes.ActionUnknown);
            }

            feats.Add(feat);
        }

        character.TrainingProgress.TryGetValue(projectId, out int current);
        int progress = Math.Min(project.WorkUnits, checked(current + workUnits));
        if (progress < project.WorkUnits)
        {
            CharacterState partial = character with { TrainingProgress = character.TrainingProgress.SetItem(projectId, progress) };
            return new TrainingContributionResult(partial, true, ActionRejectionCodes.None, progress, null);
        }

        CharacterCapabilities capabilities = character.Capabilities;
        foreach (FeatDefinition feat in feats)
        {
            capabilities = capabilities.WithTrainingGrants(feat, projectId);
        }

        ImmutableArray<AccessId> access = [.. feats.SelectMany(value => value.GrantedAccessIds).Distinct().Order()];
        CharacterState completed = character with
        {
            Capabilities = capabilities,
            TrainingProgress = character.TrainingProgress.Remove(projectId),
        };
        TrainingCompletionEvent completion = new(character.Id, projectId, [.. feats.Select(value => value.FeatId).Order()], access);
        return new TrainingContributionResult(completed, true, ActionRejectionCodes.None, progress, completion);
    }

    private static TrainingContributionResult Rejected(CharacterState character, string code) =>
        new(character, false, code, 0, null);
}
