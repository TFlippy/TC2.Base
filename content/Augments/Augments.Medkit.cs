using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterMedkitAugments(ref List<Augment.Definition> definitions)
		{
			definitions.Add(Augment.Definition.New<Medkit.Data>
			(
				identifier: "medkit.alcohol",
				category: "Medkit",
				name: "Alcohol Disinfectant",
				description: "Adds an alcohol disinfectant, increasing healing amount at cost of extra pain.",

				can_add: static (ref Augment.Context context, in Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.pain += 250.00f;
					data.power *= 2.50f;
					context.requirements_new.Add(Crafting.Requirement.Resource("alcohol", 5));
				}
			));

			definitions.Add(Augment.Definition.New<Medkit.Data>
			(
				identifier: "medkit.extra_bandages",
				category: "Medkit",
				name: "Extra Bandages",
				description: "Increases area of effect.",

				can_add: static (ref Augment.Context context, in Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.aoe *= 1.50f;
					data.max_distance += 0.50f;
					context.requirements_new.Add(Crafting.Requirement.Resource("cloth", 8));
				}
			));

			definitions.Add(Augment.Definition.New<Medkit.Data>
			(
				identifier: "medkit.sterile",
				category: "Medkit",
				name: "Sterile Surgical Tools",
				description: "Allows treating more severe injuries.",

				can_add: static (ref Augment.Context context, in Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.heal_min_integrity -= MathF.Min(0.15f, data.heal_min_integrity);
					data.heal_min_durability -= MathF.Min(0.10f, data.heal_min_durability);

					var ingot_amount = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Ingot))
							{
								ingot_amount += requirement.amount;
								requirement = default;
							}
						}
					}

					var total_amount = 1.00f + ingot_amount;
					context.requirements_new.Add(Crafting.Requirement.Resource("silver_ingot", total_amount));
				}
			));

			//definitions.Add(Augment.Definition.New<Medkit.Data>
			//(
			//	identifier: "medkit.replacement_skin",
			//	category: "Medkit",
			//	name: "Replacement Skin",
			//	description: "Graft skin and flesh (Very painfull)",

			//	can_add: static (ref Augment.Context context, in Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		return !augments.HasAugment(handle);
			//	},

			//	apply_1: static (ref Augment.Context context, ref Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		data.cooldown *= 1.20f;
			//		data.power *= 1.70f;
			//		data.heal_min = MathF.Min(1.00f, data.heal_min + 0.05f);
			//		data.pain += 300.00f;
			//		context.requirements_new.Add(Crafting.Requirement.Resource("meat", 150));
			//	}
			//));

			//definitions.Add(Augment.Definition.New<Medkit.Data>
			//(
			//	identifier: "medkit.mithril_treatment",
			//	category: "Medkit",
			//	name: "Experimental Mithril Treatment",
			//	description: "Mithril does wonders to the flesh, though the reaction is a bit strange.",

			//	can_add: static (ref Augment.Context context, in Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		return !augments.HasAugment(handle);
			//	},

			//	apply_1: static (ref Augment.Context context, ref Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		data.power *= 1.50f; // Power is justified due to mithril being hideously expensive
			//		data.pain += 100.00f;

			//		data.critical_heal += 3.00f; // Quadruple healing at 0 hp, 2.5x healing at 50% hp
			//		data.heal_min_integrity += 0.05f;
			//		data.heal_min_durability -= 0.20f;

			//		context.requirements_new.Add(Crafting.Requirement.Resource("mithril", 3));
			//	}
			//));

			//definitions.Add(Augment.Definition.New<Medkit.Data>
			//(
			//	identifier: "medkit.blue_mushrooms",
			//	category: "Medkit",
			//	name: "Blue Mushroom Treatment",
			//	description: "Blue Mushroom",

			//	can_add: static (ref Augment.Context context, in Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		return !augments.HasAugment(handle);
			//	},

			//	apply_1: static (ref Augment.Context context, ref Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		data.power *= 1.20f;
			//		data.cooldown *= 0.70f;
			//		data.pain -= 100.00f;

			//		data.critical_heal -= 2.00f; // No healing at half hp or lower

			//		context.requirements_new.Add(Crafting.Requirement.Resource("mushroom.blue", 100));
			//	}
			//));

			definitions.Add(Augment.Definition.New<Medkit.Data>
			(
				identifier: "medkit.red_sugar",
				category: "Medkit",
				name: "Red Sugar Anesthetic",
				description: "Red sugar numbs the pain with sweetness.",

				can_add: static (ref Augment.Context context, in Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Medkit.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.pain -= 200.00f;
					context.requirements_new.Add(Crafting.Requirement.Resource("red_sugar", 10));
				}
			));
		}
	}
}

