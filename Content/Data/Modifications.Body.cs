﻿using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterBodyModifications(ref List<Modification.Definition> definitions)
		{
			definitions.Add(Modification.Definition.New<Body.Data>
			(
				identifier: "body.bulk",
				name: "Batch Production",
				description: "More efficient manufacturing process by producing multiple items in bulk.",

				validate: static (ref Modification.Context context, in Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var batch_size = ref handle.GetData<int>();
					batch_size = Maths.Clamp(batch_size, 1, 10);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var batch_size = ref handle.GetData<int>();
					return GUI.SliderInt("##stuff", ref batch_size, 1, 10, "%d");
				},
#endif

				can_add: static (ref Modification.Context context, in Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref readonly var recipe_old = ref context.GetRecipeOld();
					ref var recipe_new = ref context.GetRecipeNew();

					ref var batch_size = ref handle.GetData<int>();

					recipe_new.min *= batch_size;
					recipe_new.max *= batch_size;
					recipe_new.step *= batch_size;

					for (int i = 0; i < context.requirements_new.Length; i++)
					{
						ref var requirement = ref context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= MathF.Pow(0.99f, batch_size - 1);
							}
							else
							{
								requirement.amount *= MathF.Pow(0.98f, batch_size - 1);
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= MathF.Pow(0.95f, batch_size - 1);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= MathF.Pow(0.93f, batch_size - 1);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= MathF.Pow(0.90f, batch_size - 1);
								}
								break;

								default:
								{
									requirement.amount *= MathF.Pow(0.95f, batch_size - 1);
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Body.Data> // Can be used on any recipe which results in a prefab
			(
				identifier: "body.efficient_crafting",
				category: "Body",
				name: "Efficient Crafting",
				description: "Rework the design to reduce material costs slightly.",
				
				//This modifier is nearly always an option but only with many npcs or very high skills is this actually worth while since it ramps up crafting time a ton

				apply_1: static (ref Modification.Context context, ref Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 0.95f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.50f;
							requirement.difficulty += 3.00f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Body.Data>
			(
				identifier: "body.lightweight_design",
				category: "Body",
				name: "Lightweight Design",
				description: "Redesign the object to have slightly less weight but be harder to produce",

				can_add: static (ref Modification.Context context, in Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.mass_multiplier *= 0.80f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.30f;
							requirement.difficulty += 2.0f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Body.Data>
			(
				identifier: "body.heavyweight_design",
				category: "Body",
				name: "Heavyweight Design",
				description: "Redesign the object to be a lot heavier",

				apply_1: static (ref Modification.Context context, ref Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.mass_multiplier *= 2.00f; 
					//This modifier is mostly a negative but sometimes you may want to increase the weight of an object like,
					//For example you can increase the weight of your own melee weapons to make them harder to grab for your opponents
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.10f;
						}
					}
				}
			));
		}
	}
}

