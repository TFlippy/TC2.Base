﻿namespace TC2.Base
{
	public sealed partial class BaseMod
	{
		private static void RegisterArmorAugments(ref List<Augment.Definition> definitions)
		{
			definitions.Add(Augment.Definition.New<Health.Data>
			(
				identifier: "health.smirglum_structure",
				category: "Protection",
				name: "Smirglum-Reinforced Structure",
				description: "Replaces entire structure with smirglum, greatly increasing durability and making it solid to essences.",
				flags: Augment.Definition.Flags.Hidden,

				can_add: static (ref Augment.Context context, in Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					var h_mat = new IMaterial.Handle("steel.ingot");

					var has_ingot = false;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							if (requirement.material == h_mat)
							{
								has_ingot = true;
								break;
							}
						}
					}

					return has_ingot && !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					var h_mat = new IMaterial.Handle("steel.ingot");

					var ingot_amount = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.50f;
							requirement.difficulty += 5;
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							if (requirement.material == h_mat)
							{
								ingot_amount += requirement.amount;
								requirement = default;
							}
						}
					}

					var total_amount = 3.00f + (ingot_amount * 0.30f);
					context.requirements_new.Merge(Crafting.Requirement.Resource("smirglum.plate", total_amount).WithFlags(Crafting.Requirement.Flags.Prerequisite | Crafting.Requirement.Flags.Compact));

					data.max += total_amount * 1000.00f;

					ref var armor = ref context.GetOrAddComponent<Armor.Data>();
					if (!armor.IsNull())
					{
						armor.material_type = Material.Type.Metal;
						armor.toughness += 200.00f;
					}

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref IMaterial.Database.GetData("smirglum.plate");
						if (material.IsNotNull())
						{
							context.mass_new += total_amount * material.mass_per_unit * 0.70f;
							body.override_shape_mask |= Physics.Layer.Essence;
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Armor.Data>
			(
				identifier: "armor.chitin_lined",
				category: "Protection",
				name: "Chitin-Lined",
				description: "Incorporate chitin lining into the armor, increasing its protection while slightly lowering its toughness.",

				can_add: static (ref Augment.Context context, in Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.toughness *= 0.95f;
					data.protection = Maths.Clamp(data.protection + ((1.00f - data.protection) * 0.50f), 0.00f, 1.00f);

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max *= 0.95f;
					}
				},

				apply_1: static (ref Augment.Context context, ref Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Merge(Crafting.Requirement.Resource("chitin", 10.00f).WithFlags(Crafting.Requirement.Flags.Prerequisite | Crafting.Requirement.Flags.Compact));
				}
			));

			definitions.Add(Augment.Definition.New<Armor.Data>
			(
				identifier: "armor.smirglum_plating",
				category: "Protection",
				name: "Smirglum Plating",
				description: "Reinforce the armor with smirglum plating, increasing its toughness, weight and making it solid to essences.",
				flags: Augment.Definition.Flags.Hidden,

				can_add: static (ref Augment.Context context, in Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.toughness += 350.00f;

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max += 500.00f;
					}

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						context.mass_new += 5.00f;
						body.override_shape_mask |= Physics.Layer.Essence;
					}
				},

				apply_1: static (ref Augment.Context context, ref Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Merge(Crafting.Requirement.Resource("smirglum.plate", 0.50f).WithFlags(Crafting.Requirement.Flags.Prerequisite | Crafting.Requirement.Flags.Compact));
				}
			));
		}
	}
}

