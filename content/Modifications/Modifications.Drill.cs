using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		// TODO: Split this into multiple files
		private static void RegisterDrillModifications(ref List<Modification.Definition> definitions)
		{
			definitions.Add(Modification.Definition.New<Drill.Data>
			(
				identifier: "drill.overclocked_mechanism",
				name: "Overclocked Mechanism",
				description: "Increases drilling speed.",

				can_add: static (ref Modification.Context context, in Drill.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Drill.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.speed *= 1.60f;
				},

				apply_1: static (ref Modification.Context context, ref Drill.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.30f;
							requirement.difficulty += 3.00f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 1.20f;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Drill.Data>
			(
				identifier: "drill.larger_drill_head",
				name: "Larger Drill Head",
				description: "Increases drilling area, but reduces speed.",

				can_add: static (ref Modification.Context context, in Drill.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Drill.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.speed *= 0.70f;
					data.radius *= 2.00f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 1.30f;
					}
				},

				apply_1: static (ref Modification.Context context, ref Drill.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.50f;
							requirement.difficulty += 1.00f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 1.50f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Drill.Data>
			(
				identifier: "drill.smirgl_head",
				name: "Smirgl Drill Head",
				description: "Greatly increases drill power at cost of reduced speed.",

				can_add: static (ref Modification.Context context, in Drill.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Drill.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage *= 1.90f;
					data.speed *= 0.80f;
				},

				apply_1: static (ref Modification.Context context, ref Drill.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.20f;
							requirement.difficulty += 2.00f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Ingot))
							{
								requirement.amount *= 1.25f;
							}
						}
					}

					var amount = 5.00f;
					context.requirements_new.Add(Crafting.Requirement.Resource("smirgl_ingot", amount));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("smirgl_ingot");
						body.mass_multiplier *= 1.10f;
						body.mass_extra += amount * material.mass_per_unit;
					}
				}
			));
		}
	}
}

