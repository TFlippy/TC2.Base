using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterBodyAugments(ref List<Augment.Definition> definitions)
		{

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.efficient_crafting",
				category: "Crafting",
				name: "Efficient Crafting",
				description: "Rework the design to reduce material costs slightly.",
				
				//This modifier is nearly always an option but only with many npcs or very high skills is this actually worth while since it ramps up crafting time a ton

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 0.95f; //Tiny cost reduction effect
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.50f; //Large work cost increase
							requirement.difficulty += 3.00f;
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.lightweight_design",
				category: "Body",
				name: "Lightweight Design",
				description: "Redesign the object to have slightly less weight.",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					return !Augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
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

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.heavyweight_design",
				category: "Body",
				name: "Heavyweight Design",
				description: "Redesign the object to be a lot heavier.",

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
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

			//definitions.Add(Augment.Definition.New<Body.Data>
			//(
			//	identifier: "body.floaty",
			//	category: "Body",
			//	name: "Floaty",
			//	description: "Use motion pellets to partially counteract gravity on an object.",
			//
			//	can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
			//	{
			//		return !Augments.HasAugment(handle);
			//	},
			//
			//	apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
			//	{
			//		data.gravity *= 0.20f; 
			//		//Very fun modifer which makes an object behave very floaty
			//
			//		foreach (ref var requirement in context.requirements_new)
			//		{
			//			if (requirement.type == Crafting.Requirement.Type.Work)
			//			{
			//				requirement.amount *= 1.30f;
			//			}
			//		}
			//		context.requirements_new.Add(Crafting.Requirement.Resource("pellet.motion", 2.00f)); // Low ish cost, but effect is not very usefull in nearly all cases
			//	}
			//));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "health.mushroom_glow",
				category: "Utility",
				name: "Mushroom Glow",
				description: "Glows in the dark.",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !context.HasComponent<Light.Data>() && !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var light = ref context.GetOrAddComponent<Light.Data>();
					light.color = new Vector4(0.600f, 1.000f, 0.400f, 1.250f);
					light.scale = new Vector2(32.000f, 32.000f);
					light.intensity = 1.000f;
					light.texture = "light_invsqr";
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("mushroom.green", 10.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.mushroom_wood",
				category: "Crafting",
				name: "Mushroom Wood",
				description: "Uses mushroom scraps as a wood replacement.",

				//the purpose of this modifier is to add another use to mushroom scraps AND to allow people to use spare mushrooms as wood
				//this modifier is likely to be more usefull once buildings can be blueprinted

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					return context.requirements_new.Has(Crafting.Requirement.Resource("wood", 0.00f)) && !Augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
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

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.bulk",
				category: "Crafting",
				name: "Batch Production",
				description: "More efficient manufacturing process by producing multiple items in bulk.",

				validate: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var batch_size = ref handle.GetData<int>();
					batch_size = Maths.Clamp(batch_size, 1, 10);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var batch_size = ref handle.GetData<int>();
					return GUI.SliderInt("Count", ref batch_size, 1, 10);
				},
#endif

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
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

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.recycled",
				category: "Crafting",
				name: "Scrap-Recycled",
				description: "Lowers the production cost significantly, while also reducing overall item quality.",

				validate: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var ratio = ref handle.GetData<float>();
					ratio = Maths.Clamp(ratio, 0.10f, 0.65f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var ratio = ref handle.GetData<float>();
					return GUI.SliderFloat("Ratio", ref ratio, 0.10f, 0.65f);
				},
#endif

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					if (augments.HasAugment(handle)) return false;

					var has_valid_material = false;

					for (int i = 0; i < context.requirements_new.Length; i++)
					{
						ref var requirement = ref context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.type == Material.Type.Metal || material.type == Material.Type.Wood || material.type == Material.Type.Fabric || material.type == Material.Type.Rubber)
							{
								has_valid_material = true;
								break;
							}
						}
					}

					return has_valid_material;
				},

				apply_0: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var ratio = ref handle.GetData<float>();

					ref var armor = ref context.GetComponent<Armor.Data>();
					if (!armor.IsNull())
					{
						armor.toughness *= 1.00f - ratio;
						armor.protection *= 1.00f - ratio;

						if (ratio > 0.15f)
						{
							armor.pain_modifier *= 1.00f + ratio;
						}
					}

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max *= MathF.Pow(1.00f - ratio, 1.20f);
					}

					ref var gun = ref context.GetComponent<Gun.Data>();
					if (!gun.IsNull())
					{
						gun.stability *= MathF.Pow(1.00f - ratio, 1.30f);
						gun.failure_rate *= MathF.Pow(1.00f - ratio, 1.50f);

						if (ratio > 0.10f)
						{
							gun.failure_rate = MathF.Max(0.00f, gun.failure_rate + (ratio * 0.50f));
						}

						gun.reload_interval *= 1.00f + (ratio * 0.20f);
						gun.velocity_multiplier *= 1.00f - (ratio * 0.05f);
						gun.jitter_multiplier += ratio * 1.50f;
					}

					ref var melee = ref context.GetComponent<Melee.Data>();
					if (!melee.IsNull())
					{
						melee.damage_base *= 1.00f - ratio;
						melee.damage_bonus *= MathF.Pow(1.00f + ratio, 1.10f);
					}

					ref var holdable = ref context.GetComponent<Holdable.Data>();
					if (!holdable.IsNull())
					{
						holdable.force_multiplier *= MathF.Pow(1.00f - (ratio * 0.50f), 1.30f);
						holdable.torque_multiplier *= MathF.Pow(1.00f - (ratio * 0.10f), 1.20f);
					}

					ref var attachable = ref context.GetComponent<Attachable.Data>();
					if (!attachable.IsNull())
					{
						attachable.force_multiplier *= MathF.Pow(1.00f - (ratio * 0.40f), 1.20f);
						attachable.torque_multiplier *= MathF.Pow(1.00f - (ratio * 0.05f), 1.20f);
					}
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var ratio = ref handle.GetData<float>();

					ref var material_scrap = ref Material.GetMaterial("scrap");
					var total_mass = 0.00f;

					for (int i = 0; i < context.requirements_new.Length; i++)
					{
						ref var requirement = ref context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.type == Material.Type.Metal || material.type == Material.Type.Wood || material.type == Material.Type.Fabric || material.type == Material.Type.Rubber)
							{
								if (material.flags.HasAll(Material.Flags.Manufactured))
								{
									var removed_amount = requirement.amount * (ratio);
									requirement.amount -= removed_amount;

									total_mass += material.mass_per_unit * removed_amount * 2.10f;
								}
								else
								{
									var removed_amount = requirement.amount * (ratio * 0.70f);
									requirement.amount -= removed_amount;

									total_mass += material.mass_per_unit * removed_amount * 1.70f;
								}
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= MathF.Pow(1.00f - ratio, 1.50f);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= MathF.Pow(1.00f - ratio, 1.30f);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= MathF.Pow(1.00f - (ratio * 0.80f), 1.10f);
								}
								break;
							}
						}
					}

					context.requirements_new.Add(Crafting.Requirement.Resource(material_scrap.id, total_mass / material_scrap.mass_per_unit));
				}
			));
		}
	}
}

