using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterBodyModifications(ref List<Modification.Definition> definitions)
		{

			definitions.Add(Modification.Definition.New<Body.Data>
			(
				identifier: "body.efficient_crafting",
				category: "Crafting",
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
					data.gravity *= 0.90f;
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
					data.gravity *= 1.10f;
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

			definitions.Add(Modification.Definition.New<Body.Data>
			(
				identifier: "body.floaty",
				category: "Body",
				name: "Floaty",
				description: "Makes the object fall slower",

				can_add: static (ref Modification.Context context, in Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.gravity *= 0.20f; 
					//Very fun modifer which makes an object behave very floaty

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.30f;
						}
					}
					context.requirements_new.Add(Crafting.Requirement.Resource("salt.motion", 2.00f)); // Low ish cost, but effect is not very usefull in nearly all cases
				}
			));

			definitions.Add(Modification.Definition.New<Body.Data>
			(
				identifier: "body.mushroom_wood",
				category: "Crafting",
				name: "Mushroom Wood",
				description: "Uses mushroom scraps as a wood replacement",

				//the purpose of this modifier is to add another use to mushroom scraps AND to allow people to use spare mushrooms as wood
				//this modifier is likely to be more usefull once buildings can be blueprinted

				can_add: static (ref Modification.Context context, in Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return context.requirements_new.Has(Crafting.Requirement.Resource("wood", 0.00f)) && !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var wood_amount = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 0.50f;
							requirement.difficulty += 1.00f; 
							//Mushroom scraps are faster to process but require you to be a bit more experienced
							//This is also fine balance wise since mushroom scraps are much harder to get than wood
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.identifier == "wood")
							{
								wood_amount += requirement.amount*0.50f; //It costs excactly half as much but mushroom scraps are a ton more expensive
								requirement = default;
							}
						}
					}
					context.requirements_new.Add(Crafting.Requirement.Resource("mushroom", wood_amount));
				}
			));
		}
	}
}

