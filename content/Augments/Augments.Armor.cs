namespace TC2.Base
{
	public sealed partial class BaseMod
	{
		private static void RegisterArmorAugments(ref List<Augment.Definition> definitions)
		{
			definitions.Add(Augment.Definition.New<Health.Data>
			(
				identifier: "health.smirgl_structure",
				category: "Protection",
				name: "Smirgl-Reinforced Structure",
				description: "Replaces entire structure with smirgl, greatly increasing durability and making it solid to essences.",

				can_add: static (ref Augment.Context context, in Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var material_iron = ref Material.GetMaterial("iron_ingot");

					var has_ingot = false;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							if (requirement.material.id == material_iron.id)
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
					ref var material_iron = ref Material.GetMaterial("iron_ingot");

					var ingot_amount = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.50f;
							requirement.difficulty += 5.00f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							if (requirement.material.id == material_iron.id)
							{
								ingot_amount += requirement.amount;
								requirement = default;
							}
						}
					}

					var total_amount = 3.00f + (ingot_amount * 0.30f);
					context.requirements_new.Add(Crafting.Requirement.Resource("smirgl_ingot", total_amount));

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
						ref var material = ref Material.GetMaterial("smirgl_ingot");
						body.mass_extra += total_amount * material.mass_per_unit * 0.70f;
						body.override_shape_mask |= Physics.Layer.Essence;
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
					context.requirements_new.Add(Crafting.Requirement.Resource("chitin", 10.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Armor.Data>
			(
				identifier: "armor.smirgl_plating",
				category: "Protection",
				name: "Smirgl Plating",
				description: "Reinforce the armor with smirgl plating, increasing its toughness, weight and making it solid to essences.",

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
						body.mass_extra += 5.00f;
						body.override_shape_mask |= Physics.Layer.Essence;
					}
				},

				apply_1: static (ref Augment.Context context, ref Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("smirgl_ingot", 2.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Armor.Data>
			(
				identifier: "armor.cloth_padded",
				category: "Protection",
				name: "Cloth Padding",
				description: "Pads the inner side of the armor with cloth.",

				can_add: static (ref Augment.Context context, in Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.knockback_modifier *= 0.65f;
					data.pain_modifier *= 0.80f;
				},

				apply_1: static (ref Augment.Context context, ref Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("cloth", 5.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Armor.Data>
			(
				identifier: "armor.mushroom_stuffed",
				category: "Protection",
				name: "Mushroom Stuffing",
				description: "Stuffs the inner side of the armor with soft mushroom bits.",

				can_add: static (ref Augment.Context context, in Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.knockback_modifier = MathF.Max(data.knockback_modifier - 0.30f, data.knockback_modifier * 0.25f);
					data.pain_modifier = MathF.Max((data.pain_modifier * 0.90f) - 0.30f, data.pain_modifier * 0.10f);
				},

				apply_1: static (ref Augment.Context context, ref Armor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("mushroom", 20.00f));
				}
			));
		}
	}
}

