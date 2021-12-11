using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		// TODO: Split this into multiple files
		private static void RegisterModifications(ref List<Modification.Definition> definitions)
		{
			//definitions.Add(Modification.Definition.New<Health.Data>
			//(
			//	identifier: "health.reinforced_structure",
			//	name: "Reinforced Structure",
			//	description: "Increases durability.",

			//	can_add: static (ref Modification.Context context, in Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		var count = 0;
			//		for (int i = 0; i < modifications.Length; i++)
			//		{
			//			if (modifications[i].id == handle.id) count++;
			//		}
			//		return count < 1;
			//	},

			//	apply_0: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		data.max *= 1.20f;
			//	},

			//	apply_1: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		foreach (ref var requirement in context.requirements_new)
			//		{
			//			if (requirement.type == Crafting.Requirement.Type.Work)
			//			{
			//				requirement.amount *= 1.20f;
			//			}
			//		}
			//	}
			//));

			definitions.Add(Modification.Definition.New<Health.Data>
			(
				identifier: "health.varnish",
				category: "Health",
				name: "Varnish",
				description: "Applies varnish on item's surface, improving its durability.",

				can_add: static (ref Modification.Context context, in Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return context.requirements_new.Has(Crafting.Requirement.Resource("wood", 0.00f)) && !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var amount = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.type == Material.Type.Wood)
							{
								amount += requirement.amount;
							}
						}
					}

					var total_amount = 3.00f + (amount * 0.10f);
					context.requirements_new.Add(Crafting.Requirement.Resource("resin", total_amount));

					data.max += total_amount * 85.00f;
					data.dt += 50.00f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("resin");
						body.mass_extra += total_amount * material.mass_per_unit * 0.30f;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Health.Data>
			(
				identifier: "health.smirgl_structure",
				category: "Health",
				name: "Smirgl-Reinforced Structure",
				description: "Replaces entire structure with smirgl, greatly increasing durability.",

				can_add: static (ref Modification.Context context, in Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var ingot_amount = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 2.00f;
							requirement.difficulty += 4.00f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Ingot))
							{
								ingot_amount += requirement.amount;
								requirement = default;
							}
						}
					}

					var total_amount = 3.00f + (ingot_amount * 0.30f);
					context.requirements_new.Add(Crafting.Requirement.Resource("smirgl_ingot", total_amount));

					data.max += total_amount * 2500.00f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("smirgl_ingot");
						body.mass_extra += total_amount * material.mass_per_unit * 0.70f;
					}
				}
			));

			//definitions.Add(Modification.Definition.New<Body.Data> // Can be used on any recipe which results in a prefab
			//(
			//	identifier: "body.efficient_crafting",
			//	name: "Efficient Crafting",
			//	description: "Rework the design to reduce material costs slightly.",

			//	apply_1: static (ref Modification.Context context, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		foreach (ref var requirement in context.requirements_new)
			//		{
			//			if (requirement.type == Crafting.Requirement.Type.Resource)
			//			{
			//				requirement.amount *= 0.80f;
			//			}
			//			else if (requirement.type == Crafting.Requirement.Type.Work)
			//			{
			//				requirement.amount *= 1.05f;
			//				requirement.difficulty += 2.50f;
			//			}
			//		}
			//	}
			//));

			definitions.Add(Modification.Definition.New<Fuse.Data>
			(
				identifier: "fuse.length",
				category: "Explosives",
				name: "Fuse Length",
				description: "Modifies fuse's length.",

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("##stuff", ref value, 0.50f, 10.00f, "%.2f");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					data.time = value;
				}
			));

			definitions.Add(Modification.Definition.New<Fuse.Data>
			(
				identifier: "fuse.inextinguishable",
				category: "Explosives",
				name: "Inextinguishable Fuse",
				description: "Makes the fuse impossible to be extinguished.",

				apply_0: static (ref Modification.Context context, ref Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_chance = 0.00f;
				}
			));

			definitions.Add(Modification.Definition.New<Explosive.Data>
			(
				identifier: "explosive.directed",
				category: "Explosives",
				name: "Directed Explosion",
				description: "Focuses the explosion into a smaller area.",

				apply_0: static (ref Modification.Context context, ref Explosive.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					//var radius_log = MathF.Log2(data.radius);

					data.radius *= 0.75f;
					data.power *= 1.25f;
					data.damage_terrain *= 1.25f;
					data.damage_entity *= 1.25f;
				}
			));

			definitions.Add(Modification.Definition.New<Explosive.Data>
			(
				identifier: "explosive.nitroglycerine",
				category: "Explosives",
				name: "Nitroglycerine Filler",
				description: "Replaces filler with nitroglycerine.",

				apply_0: static (ref Modification.Context context, ref Explosive.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var material_nitroglycerine_handle = new Material.Handle("nitroglycerine");
					ref var material_nitroglycerine = ref material_nitroglycerine_handle.GetDefinition();

					var has_any = false;

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (requirement.material.id != material_nitroglycerine_handle.id && material.flags.HasAny(Material.Flags.Explosive))
							{
								var amount_new = (requirement.amount * material.mass_per_unit) / material_nitroglycerine.mass_per_unit;

								requirement.amount = amount_new;
								requirement.material = material_nitroglycerine_handle;

								has_any = true;
							}
						}
					}

					if (has_any)
					{
						data.radius += MathF.Sqrt(data.radius * 1.50f);
						data.power += MathF.Sqrt(data.power * 2.50f);
						data.damage_terrain += MathF.Pow(data.damage_terrain * 3.50f, 0.75f);
						data.damage_entity += MathF.Pow(data.damage_entity * 2.50f, 0.75f);
					}
				}
			));

			definitions.Add(Modification.Definition.New<Overheat.Data>
			(
				identifier: "overheat.coolant",
				category: "Cooling",
				name: "Water-Cooled",
				description: "Increases cooling rate.",

				validate: static (ref Modification.Context context, in Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();
					amount = Maths.Clamp(amount, 1, 40);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();
					return GUI.SliderInt("##stuff", ref amount, 1, 40, "%d");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();
					data.cool_rate += amount * 4.00f;
				},

				apply_1: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();

					context.requirements_new.Add(Crafting.Requirement.Resource("water", amount));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("water");
						body.mass_extra += amount * material.mass_per_unit;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Overheat.Data>
			(
				identifier: "overheat.heat_resistant",
				category: "Cooling",
				name: "Heat-Resistant Components",
				description: "Dramatically increases maximum operating temperature at cost of extra weight and reduced cooling rate.",

				can_add: static (ref Modification.Context context, in Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.cool_rate *= 0.80f;
					data.heat_critical += 500.00f;
				},

				apply_1: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var amount = 0.00f;

					foreach (ref readonly var requirement in context.requirements_old)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAny(Material.Flags.Manufactured | Material.Flags.Metal))
							{
								amount += requirement.amount * 0.20f;
							}
						}
					}

					amount = MathF.Ceiling(amount);
					context.requirements_new.Add(Crafting.Requirement.Resource("smirgl_ingot", amount));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("smirgl_ingot");
						body.mass_extra += amount * material.mass_per_unit;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.expanded_magazine",
				name: "Expanded Magazine",
				description: "Increases ammo capacity.",

				validate: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 1, 5);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					return GUI.SliderFloat("##stuff", ref amount, 1.00f, 5.00f, "%.2f");
				},
#endif

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.feed == Gun.Feed.Single) return false;
					return !modifications.HasModification(handle);
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var amount = handle.GetData<float>();
					var mass = 0.00f;

					switch (data.feed)
					{
						case Gun.Feed.Drum:
						{
							amount *= 10.00f;
							mass += amount * 0.60f;
						}
						break;

						case Gun.Feed.Cylinder:
						{
							amount += amount * 0.30f;
							mass += amount * 0.10f;
						}
						break;

						case Gun.Feed.Clip:
						{
							amount += amount * 2.00f;
							mass += amount * 0.20f;
						}
						break;

						case Gun.Feed.Magazine:
						{
							amount += amount * 5.00f;
							mass += amount * 0.30f;
						}
						break;
					}

					switch (data.type)
					{
						case Gun.Type.Cannon:
						{
							amount *= 2.00f;
							mass *= 3.00f;
						}
						break;

						case Gun.Type.AutoCannon:
						{
							amount *= 3.00f;
							mass *= 2.00f;
						}
						break;

						case Gun.Type.MachineGun:
						{
							amount *= 1.20f;
							mass *= 1.20f;
						}
						break;
					}

					if (data.ammo_filter.HasAll(Material.Flags.Ammo_Shell))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 0.20f));
						mass *= 4.00f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_AC))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 0.30f));
						mass *= 2.50f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_Rocket))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 0.40f));
						mass *= 1.20f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 0.60f));
						mass *= 1.20f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_SG))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 0.85f));
						mass *= 1.10f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 1.00f));
						mass *= 1.00f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_LC))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 1.50f));
						mass *= 0.90f;
					}

					data.max_ammo += amount;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_extra += mass;
					}

					return true;
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref readonly var requirement in context.requirements_old)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (!material.flags.HasAll(Material.Flags.Manufactured) && material.type == Material.Type.Metal)
							{
								context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.30f));
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.improved_rifling",
				name: "Improved Rifling",
				description: "Improves accuracy and damage.",

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("##stuff", ref value, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					var ratio = value;

					switch (data.type)
					{
						case Gun.Type.Shotgun:
						{
							data.velocity_multiplier *= Maths.Lerp(1.00f, 0.60f, ratio);
							data.damage_multiplier *= Maths.Lerp(1.00f, 0.85f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.50f, ratio);
						}
						break;

						case Gun.Type.Rifle:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.19f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.04f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.70f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.20f, ratio);
						}
						break;

						case Gun.Type.SMG:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.15f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.04f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.70f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.30f, ratio);
						}
						break;

						case Gun.Type.Handgun:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.13f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.05f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.60f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.10f, ratio);
						}
						break;

						default:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.06f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.03f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.30f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.20f, ratio);
						}
						break;
					}

					switch (data.action)
					{
						case Gun.Action.Blowback:
						{
							data.cycle_interval *= Maths.Lerp(1.00f, 1.19f, ratio);
						}
						break;

						case Gun.Action.Gas:
						{
							data.cycle_interval *= Maths.Lerp(1.00f, 1.11f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.003f, ratio);
							data.failure_rate *= Maths.Lerp(1.00f, 1.15f, ratio);
						}
						break;
					}
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.difficulty += 3.00f;
									requirement.amount += 100.00f;
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.difficulty *= 1.50f;
									requirement.amount += 80.00f;
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.automatic",
				name: "Mode: Automatic",
				description: "Converts fire mode to automatic.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.action == Gun.Action.Gas || data.action == Gun.Action.Blowback || data.action == Gun.Action.Crank;
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (!data.flags.HasAll(Gun.Flags.Automatic))
					{
						switch (data.action)
						{
							case Gun.Action.Gas:
							{
								data.flags |= Gun.Flags.Automatic;

								data.cycle_interval *= 1.10f;

								data.failure_rate *= 1.50f;
								data.failure_rate += 0.02f;

								return true;
							}
							break;

							case Gun.Action.Blowback:
							case Gun.Action.Crank:
							{
								data.flags |= Gun.Flags.Automatic;

								data.cycle_interval *= 1.30f;

								data.failure_rate *= 2.50f;
								data.failure_rate += 0.05f;

								return true;
							}
							break;
						}
					}

					return false;
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.semi_automatic",
				name: "Mode: Semi-Automatic",
				description: "Converts fire mode to semi-automatic.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.action == Gun.Action.Gas || data.action == Gun.Action.Blowback || data.action == Gun.Action.Crank;
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.flags.HasAll(Gun.Flags.Automatic))
					{
						switch (data.action)
						{
							case Gun.Action.Gas:
							case Gun.Action.Blowback:
							case Gun.Action.Crank:
							{
								data.flags &= ~Gun.Flags.Automatic;
								data.failure_rate *= 0.10f;
								data.failure_rate -= MathF.Min(data.failure_rate, 0.02f);

								return true;
							}
							break;
						}
					}

					return false;
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.caliber_downgrade",
				name: "Caliber Downgrade",
				description: "Rechambers to a lower caliber.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.ammo_filter.HasAny(Material.Flags.Ammo_AC | Material.Flags.Ammo_MG | Material.Flags.Ammo_HC);
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.ammo_filter.HasAll(Material.Flags.Ammo_AC))
					{
						data.ammo_filter |= Material.Flags.Ammo_MG;
						data.ammo_filter &= ~Material.Flags.Ammo_AC;

						data.damage_multiplier *= 1.50f;
						data.velocity_multiplier *= 1.10f;
						data.failure_rate *= 0.20f;
						data.failure_rate += 0.10f;
						data.recoil_multiplier *= 0.50f;
						data.jitter_multiplier += 1.50f;
						data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.80f));

						data.stability = Maths.Clamp(data.stability * 3.50f, 0.00f, 1.00f);

						data.sound_pitch *= 1.15f;
						data.sound_volume *= 1.02f;

						data.smoke_size *= 0.90f;
						data.flash_size *= 0.90f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
					{
						data.ammo_filter |= Material.Flags.Ammo_HC;
						data.ammo_filter &= ~Material.Flags.Ammo_MG;

						data.damage_multiplier *= 1.35f;
						data.velocity_multiplier *= 0.95f;
						data.failure_rate *= 1.90f;
						data.failure_rate += 0.50f;
						data.recoil_multiplier *= 0.70f;
						data.jitter_multiplier += 0.50f;
						data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.30f));

						data.stability = Maths.Clamp(data.stability * 1.20f, 0.00f, 1.00f);

						data.sound_pitch *= 1.15f;
						data.sound_volume *= 1.02f;

						data.smoke_size *= 0.90f;
						data.flash_size *= 0.90f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
					{
						data.ammo_filter |= Material.Flags.Ammo_LC;
						data.ammo_filter &= ~Material.Flags.Ammo_HC;

						data.damage_multiplier *= 1.20f;
						data.velocity_multiplier *= 0.85f;
						data.failure_rate *= 1.70f;
						data.failure_rate += 0.50f;
						data.recoil_multiplier *= 0.60f;
						data.jitter_multiplier += 0.50f;
						data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.20f));

						data.stability = Maths.Clamp(data.stability * 1.40f, 0.00f, 1.00f);

						data.sound_pitch *= 1.10f;
						data.sound_volume *= 1.02f;

						data.smoke_size *= 0.90f;
						data.flash_size *= 0.90f;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.caliber_upgrade",
				name: "Caliber Upgrade",
				description: "Rechambers to a higher caliber.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.ammo_filter.HasAny(Material.Flags.Ammo_LC | Material.Flags.Ammo_HC | Material.Flags.Ammo_MG);
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.ammo_filter.HasAll(Material.Flags.Ammo_LC))
					{
						data.ammo_filter |= Material.Flags.Ammo_HC;
						data.ammo_filter &= ~Material.Flags.Ammo_LC;

						data.damage_multiplier *= 0.90f;
						data.velocity_multiplier *= 0.90f;
						data.failure_rate *= 1.60f;
						data.failure_rate += 0.40f;
						data.recoil_multiplier *= 1.70f;
						data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 0.60f));

						data.stability -= MathF.Min(data.stability, 0.30f);
						data.stability = Maths.Clamp(data.stability * 0.80f, 0.00f, 1.00f);

						data.sound_pitch *= 0.82f;
						data.sound_volume *= 1.20f;

						data.smoke_size *= 1.30f;
						data.flash_size *= 1.10f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
					{
						data.ammo_filter |= Material.Flags.Ammo_MG;
						data.ammo_filter &= ~Material.Flags.Ammo_HC;

						data.damage_multiplier *= 0.85f;
						data.velocity_multiplier *= 0.85f;
						data.failure_rate *= 1.40f;
						data.failure_rate += 0.40f;
						data.recoil_multiplier *= 1.80f;
						data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 0.50f));

						data.stability -= MathF.Min(data.stability, 0.40f);
						data.stability = Maths.Clamp(data.stability * 0.80f, 0.00f, 1.00f);

						data.sound_pitch *= 0.87f;
						data.sound_volume *= 1.25f;

						data.smoke_size *= 1.30f;
						data.flash_size *= 1.10f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
					{
						data.ammo_filter |= Material.Flags.Ammo_AC;
						data.ammo_filter &= ~Material.Flags.Ammo_MG;

						data.damage_multiplier *= 0.70f;
						data.velocity_multiplier *= 0.50f;
						data.failure_rate *= 2.20f;
						data.failure_rate += 0.40f;
						data.recoil_multiplier *= 3.50f;
						data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 0.10f));

						data.stability -= MathF.Min(data.stability, 0.80f);
						data.stability = Maths.Clamp(data.stability * 0.70f, 0.00f, 1.00f);

						data.sound_pitch *= 0.67f;
						data.sound_volume *= 1.25f;

						data.smoke_size *= 1.50f;
						data.flash_size *= 1.70f;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.recoil_reduction",
				name: "Recoil Reduction",
				description: "Reduces recoil.",

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("##stuff", ref value, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					var ratio = value;

					switch (data.type)
					{
						case Gun.Type.Handgun:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 0.97f, ratio);
							data.recoil_multiplier = Maths.Lerp(data.recoil_multiplier, MathF.Max(0.20f, data.recoil_multiplier - 0.12f), ratio);
						}
						break;

						case Gun.Type.Rifle:
						case Gun.Type.Shotgun:
						case Gun.Type.SMG:
						case Gun.Type.MachineGun:
						case Gun.Type.AutoCannon:
						{
							data.recoil_multiplier = MathF.Max(0.10f, data.recoil_multiplier - 0.07f);
							data.recoil_multiplier *= 0.65f;
							data.velocity_multiplier *= 0.91f;
							data.damage_multiplier *= 0.94f;
							data.stability = Maths.Clamp(data.stability * 1.09f, 0.00f, 1.00f);

							switch (data.action)
							{
								case Gun.Action.Blowback:
								{
									data.cycle_interval *= 1.17f;
								}
								break;

								case Gun.Action.Gas:
								{
									data.cycle_interval *= 1.11f;
									data.failure_rate += 0.01f;
									data.failure_rate *= 1.10f;
								}
								break;
							}
						}
						break;

						default:
						{
							data.recoil_multiplier = MathF.Max(0.10f, data.recoil_multiplier - 0.07f);
							data.recoil_multiplier *= 0.60f;
							data.velocity_multiplier *= 0.87f;
							data.damage_multiplier *= 0.95f;
							data.stability = Maths.Clamp(data.stability * 1.07f, 0.00f, 1.00f);

							switch (data.action)
							{
								case Gun.Action.Blowback:
								{
									data.cycle_interval *= 1.17f;
								}
								break;

								case Gun.Action.Gas:
								{
									data.cycle_interval *= 1.11f;
									data.failure_rate += 0.01f;
									data.failure_rate *= 1.10f;
								}
								break;
							}
						}
						break;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.barrel_extension",
				name: "Barrel Extension",
				description: "Increases muzzle velocity and damage.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("##stuff", ref value, 1.00f, 2.00f, "%.2f");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					var ratio = value - 1.00f;

					switch (data.type)
					{
						case Gun.Type.Handgun:
						{
							//var mult = Maths.Clamp(data.velocity_multiplier / (300.00f * (value * value)), 0.01f, 2.00f);
							var mult = 1.00f - ratio;

							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.50f * mult, ratio);
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.45f * mult, ratio);
							data.recoil_multiplier *= Maths.Lerp(1.00f, 1.10f, value);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.20f, ratio);
						}
						break;

						case Gun.Type.Rifle:
						{
							//var mult = Maths.Clamp(data.velocity_multiplier / (800.00f * (value)), 0.01f, 2.00f);
							var mult = 1.00f - ratio;

							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.50f * mult, ratio);
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.90f * mult, ratio);
							data.recoil_multiplier *= Maths.Lerp(1.00f, 1.20f * value, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.30f * mult, ratio);
						}
						break;

						case Gun.Type.Shotgun:
						{
							data.damage_multiplier *= 1.10f;
							data.recoil_multiplier *= 1.30f;
							data.jitter_multiplier *= 0.40f;
							data.velocity_multiplier *= 1.10f;
						}
						break;

						case Gun.Type.SMG:
						{
							data.damage_multiplier *= 1.15f;
							data.recoil_multiplier *= 1.30f;
							data.jitter_multiplier *= 0.70f;
							data.velocity_multiplier *= 1.20f;
						}
						break;

						default:
						{
							data.damage_multiplier *= 1.15f;
							data.recoil_multiplier *= 1.30f;
							data.jitter_multiplier *= 0.70f;
							data.velocity_multiplier *= 1.20f;
						}
						break;
					}

					switch (data.action)
					{
						case Gun.Action.Blowback:
						{
							data.cycle_interval *= 1.20f;
						}
						break;

						case Gun.Action.Gas:
						{
							data.cycle_interval *= 1.14f;
							data.failure_rate += 0.01f;
							data.failure_rate *= 1.10f;
						}
						break;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.balanced_receiver",
				name: "Balanced Receiver",
				description: "Stabilizes the receiver, increasing reliability.",

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					switch (data.type)
					{
						case Gun.Type.Handgun:
						{
							data.damage_multiplier *= 0.98f;
							data.velocity_multiplier *= 0.97f;
							data.failure_rate *= 0.18f;
							data.cycle_interval *= 0.98f;
							data.stability = Maths.Clamp(data.stability * 1.20f, 0.00f, 1.00f);
						}
						break;

						case Gun.Type.Rifle:
						{
							data.damage_multiplier *= 0.96f;
							data.velocity_multiplier *= 0.97f;
							data.failure_rate = MathF.Max(0.00f, data.failure_rate - 0.002f);
							data.failure_rate *= 0.14f;
							data.cycle_interval *= 1.06f;
							data.stability = Maths.Clamp(data.stability * 1.10f, 0.00f, 1.00f);
						}
						break;

						case Gun.Type.Shotgun:
						{
							data.damage_multiplier *= 0.95f;
							data.velocity_multiplier *= 0.95f;
							data.failure_rate = MathF.Max(0.00f, data.failure_rate - 0.001f);
							data.failure_rate *= 0.12f;
							data.cycle_interval += 0.03f;
							data.stability = Maths.Clamp(data.stability * 1.15f, 0.00f, 1.00f);
						}
						break;

						case Gun.Type.SMG:
						{
							data.damage_multiplier *= 0.97f;
							data.velocity_multiplier *= 0.97f;
							data.failure_rate = MathF.Max(0.00f, data.failure_rate - 0.003f);
							data.failure_rate *= 0.17f;
							data.cycle_interval *= 1.06f;
							data.stability = Maths.Clamp(data.stability * 1.20f, 0.00f, 1.00f);
						}
						break;

						default:
						{
							data.damage_multiplier *= 0.96f;
							data.velocity_multiplier *= 0.97f;
							data.failure_rate = MathF.Max(0.00f, data.failure_rate - 0.001f);
							data.failure_rate *= 0.20f;
							data.cycle_interval *= 1.06f;
							data.stability = Maths.Clamp(data.stability * 1.25f, 0.00f, 1.00f);
						}
						break;
					}
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					for (int i = 0; i < context.requirements_old.Length; i++)
					{
						var requirement = context.requirements_old[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Manufactured))
							{
								context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.20f));
							}
							else if (material.flags.HasAll(Material.Flags.Metal))
							{
								context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.25f));
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= 0.20f;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= 0.20f;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.05f;
									context.requirements_new.Add(requirement);
								}
								break;
							}
						}
					}

					for (int i = 0; i < context.requirements_new.Length; i++)
					{
						var requirement = context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.difficulty += 2.00f;
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.difficulty += 2.00f;
								}
								break;
							}
						}

					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.unstable_receiver",
				name: "Unstable Receiver",
				description: "Greatly improves most of the gun's properties, at cost of reduced stability and reliability.",

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("##stuff", ref value, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();

					switch (data.type)
					{
						case Gun.Type.Handgun:
						{
							data.damage_multiplier *= Maths.Lerp(1.35f, 1.50f, value);
							data.velocity_multiplier *= 1.08f;
							data.failure_rate *= 1.70f;
							data.failure_rate += 0.06f;
							data.stability -= MathF.Min(data.stability, 0.07f);
							data.stability = Maths.Clamp(data.stability * 0.97f, 0.00f, 1.00f);
							data.cycle_interval += 0.03f;
							data.cycle_interval *= 1.15f;
							data.jitter_multiplier += 0.35f;
							data.jitter_multiplier *= 1.35f;
							data.recoil_multiplier *= 1.30f;
						}
						break;

						case Gun.Type.Rifle:
						{
							data.damage_multiplier *= Maths.Lerp(1.35f, 1.90f, value);
							data.velocity_multiplier *= 1.07f;
							data.failure_rate *= 3.70f;
							data.failure_rate += 0.04f;
							data.stability -= MathF.Min(data.stability, 0.10f);
							data.stability = Maths.Clamp(data.stability * 0.95f, 0.00f, 1.00f);
							data.cycle_interval += 0.04f;
							data.cycle_interval *= 1.10f;
							data.jitter_multiplier += 0.45f;
							data.jitter_multiplier *= 1.25f;
							data.recoil_multiplier *= 1.10f;
						}
						break;

						case Gun.Type.Shotgun:
						{
							data.damage_multiplier *= Maths.Lerp(1.31f, 1.60f, value);
							data.velocity_multiplier *= 1.04f;
							data.failure_rate *= 3.70f;
							data.failure_rate += 0.01f;
							data.stability -= MathF.Min(data.stability, 0.08f);
							data.stability = Maths.Clamp(data.stability * 0.95f, 0.00f, 1.00f);
							data.cycle_interval += 0.03f;
							data.jitter_multiplier += 0.75f;
							data.jitter_multiplier *= 1.35f;
							data.recoil_multiplier *= 1.40f;
						}
						break;

						case Gun.Type.SMG:
						{
							data.damage_multiplier *= Maths.Lerp(1.256f, 1.40f, value);
							data.velocity_multiplier *= 1.01f;
							data.failure_rate *= 4.60f;
							data.failure_rate += 0.04f;
							data.stability -= MathF.Min(data.stability, 0.05f);
							data.stability = Maths.Clamp(data.stability * 0.95f, 0.00f, 1.00f);
							data.cycle_interval *= 0.70f;
							data.cycle_interval += 0.01f;
							data.jitter_multiplier += 0.35f;
							data.jitter_multiplier *= 1.35f;
							data.recoil_multiplier *= 1.20f;
						}
						break;

						default:
						{
							data.damage_multiplier *= Maths.Lerp(1.27f, 1.85f, value);
							data.velocity_multiplier *= 1.03f;
							data.failure_rate *= 3.60f;
							data.failure_rate += 0.03f;
							data.stability -= MathF.Min(data.stability, 0.10f);
							data.stability = Maths.Clamp(data.stability * 0.95f, 0.00f, 1.00f);
							data.cycle_interval *= 0.95f;
							data.cycle_interval += 0.01f;
							data.jitter_multiplier += 0.35f;
							data.jitter_multiplier *= 1.35f;
							data.recoil_multiplier *= 1.10f;
						}
						break;
					}

					if (data.flags.HasAll(Gun.Flags.Automatic))
					{
						data.failure_rate = Maths.Clamp(data.failure_rate * 1.50f, 0.00f, 1.00f);
					}
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					for (int i = 0; i < context.requirements_old.Length; i++)
					{
						var requirement = context.requirements_old[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Manufactured))
							{
								context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.20f));
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= 0.10f;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.05f;
									context.requirements_new.Add(requirement);
								}
								break;
							}
						}
					}

					for (int i = 0; i < context.requirements_new.Length; i++)
					{
						var requirement = context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Assembling:
								{
									requirement.difficulty += 1.00f;
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.tempered_frame",
				name: "Tempered Frame",
				description: "Greatly improves durability and stability of the gun.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.80f;
					data.stability += 0.40f;
					data.stability = Maths.Clamp(data.stability * 1.50f, 0.00f, 1.00f);

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max *= 1.40f;
						health.dt += 70.00f;
					}
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate -= MathF.Min(data.failure_rate, 0.20f);
					data.failure_rate *= 0.90f;
					data.stability += 0.10f;
					data.stability = Maths.Clamp(data.stability * 1.30f, 0.00f, 1.00f);

					return true;
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					for (int i = 0; i < context.requirements_old.Length; i++)
					{
						var requirement = context.requirements_old[i];

						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 0.20f;
									requirement.difficulty += 8;
									context.requirements_new.Add(requirement);
								}
								break;
							}
						}
					}

					context.requirements_new.Add(Crafting.Requirement.Resource("phlogiston", 3.00f));
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.hardened_frame",
				name: "Hardened Frame",
				description: "Improves reliability of the gun.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.50f;
					data.stability += 0.20f;
					data.stability = Maths.Clamp(data.stability * 1.30f, 0.00f, 1.00f);
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate -= MathF.Min(data.failure_rate, 0.20f);
					data.failure_rate *= 0.70f;
					data.stability += 0.10f;
					data.stability = Maths.Clamp(data.stability * 1.10f, 0.00f, 1.00f);

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 1.30f;
					}

					return true;
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					for (int i = 0; i < context.requirements_old.Length; i++)
					{
						var requirement = context.requirements_old[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (!material.flags.HasAll(Material.Flags.Manufactured))
							{
								context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.40f));
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 0.20f;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 0.05f;
									context.requirements_new.Add(requirement);
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.flimsy_frame",
				name: "Flimsy Frame",
				description: "Lowers the basic resource cost, but makes problems more pronounced.",

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("##stuff", ref value, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();

					data.failure_rate *= Maths.Lerp(1.20f, 1.80f, value);
					data.failure_rate += Maths.Lerp(0.03f, 0.10f, value);
					data.stability *= Maths.Clamp(data.stability, 0.00f, 0.99f);
					data.stability = Maths.Clamp(data.stability - 0.03f, 0.00f, 1.00f);
					data.cycle_interval *= 1.05f;
					data.reload_interval *= 1.10f;

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max *= Maths.Lerp(0.95f, 0.70f, value);
					}
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();

					data.failure_rate += 0.03f;
					data.stability *= Maths.Clamp(data.stability, 0.00f, Maths.Lerp(0.98f, 0.95f, value));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 0.90f;
					}

					return true;
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (!material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= Maths.Lerp(0.80f, 0.60f, value);
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.simple_frame",
				name: "Simple Frame",
				description: "Simplifies the item, increasing reliability at cost of reduced performance.",

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.85f;
					data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
					data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
					data.cycle_interval *= 1.35f;
					data.reload_interval *= 0.93f;
					data.damage_multiplier *= 0.98f;
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.60f;
					data.jitter_multiplier += 1.50f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 0.90f;
					}

					return true;
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 0.78f;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 0.90f;
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 0.85f;
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 0.65f;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.60f;
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.faster_cycling_mechanism",
				name: "Faster Cycling Mechanism",
				description: "Increases rate of fire, but lowers reliability.",

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("##stuff", ref value, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					var ratio = value;

					switch (data.action)
					{
						case Gun.Action.Gas:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 2.00f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.02f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.35f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 1.40f, ratio);
						}
						break;

						case Gun.Action.Blowback:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 5.50f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.30f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.55f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 1.60f, ratio);
						}
						break;

						case Gun.Action.Revolver:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.80f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.67f, ratio);
							data.reload_interval += Maths.Lerp(0.00f, 0.04f, ratio);
						}
						break;

						case Gun.Action.Bolt:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.85f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.01f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.67f, ratio);
						}
						break;

						case Gun.Action.Pump:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.80f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.75f, ratio);
						}
						break;

						case Gun.Action.Crank:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.90f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.05f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.65f, ratio);
						}
						break;
					}

					//if (data.flags.HasAll(Gun.Flags.Automatic))
					//{
					//	data.failure_rate *= Maths.Lerp(1.00f, 2.50f, ratio);
					//}

					data.stability = Maths.Clamp(data.stability - MathF.Min(data.stability, Maths.Lerp(0.00f, (1.00f / data.cycle_interval) * 0.15f, ratio)), 0.00f, 1.00f);
					data.failure_rate += Maths.Lerp(0.00f, (1.00f / data.cycle_interval) * 0.35f, ratio);
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount += 80.00f;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount += 60.00f;
								}
								break;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.identifier == "machine_parts")
							{
								requirement.amount *= 1.20f;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.gas_operation",
				name: "Action: Gas-Operated",
				description: "Converts to gas-operated action.",

				requirements: new()
				{
					[0] = Crafting.Requirement.Level(Experience.Type.Engineering, 10, Crafting.Requirement.Flags.No_Consume),
				},

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.action == Gun.Action.Blowback;
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					switch (data.action)
					{
						case Gun.Action.Blowback:
						{
							data.damage_multiplier *= 1.25f;

							data.failure_rate *= 0.30f;
							data.cycle_interval *= 0.90f;

							data.stability += MathF.Min(data.stability, 0.15f);
							data.stability = Maths.Clamp(data.stability * 1.30f, 0.00f, 1.00f);

							data.recoil_multiplier *= 0.90f;
							data.reload_interval *= 0.80f;

							data.action = Gun.Action.Gas;

							ref var body = ref context.GetComponent<Body.Data>();
							if (!body.IsNull())
							{
								body.mass_multiplier *= 0.90f;
							}
						}
						break;
					}
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 0.90f;
							}
							else if (material.flags.HasAll(Material.Flags.Metal))
							{
								requirement.amount *= 0.85f;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= 1.10f;
									requirement.amount += 50.00f;
									requirement.difficulty += 2.00f;
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= 1.40f;
									requirement.difficulty += 3.00f;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.70f;
									requirement.difficulty += 2.00f;
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.single_shot",
				name: "Feed: Single-Shot",
				description: "Converts to single-shot feed mechanism.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.feed != Gun.Feed.Single && !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.feed = Gun.Feed.Single;
					data.max_ammo = 1;
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate = Maths.Clamp(data.failure_rate * 0.10f + (1.00f - Maths.Clamp(data.stability, 0.00f, 1.00f)), 0.00f, 1.00f);

					return true;
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					for (int i = 0; i < context.requirements_new.Length; i++)
					{
						var requirement = context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 0.50f;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= 0.50f;
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= 0.75f;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.20f;
								}
								break;
							}
						}
					}
				}
			));

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

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.extra_barrel",
				name: "Extra Barrel",
				description: "Adds an extra barrel to the gun.",

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var simultaneous = ref handle.GetData<bool>();
					return GUI.Checkbox("Simultaneous Fire", ref simultaneous, GUI.GetRemainingSpace());
				},
#endif

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var simultaneous = ref handle.GetData<bool>();

					if (simultaneous)
					{
						data.ammo_per_shot += 1.00f;
						data.smoke_amount *= 2;
						data.projectile_count += 1;
					}

					switch (data.feed)
					{
						case Gun.Feed.Single:
						{
							if (simultaneous)
							{
								data.failure_rate *= 1.10f;
								data.failure_rate += 0.25f;

								data.stability -= MathF.Min(data.stability, 0.50f);
								data.stability = Maths.Clamp(data.stability * 0.60f, 0.00f, 1.00f);

								data.reload_interval *= 1.30f;

								data.jitter_multiplier *= 1.30f;
								data.jitter_multiplier += 5.00f;

								data.sound_volume *= 1.50f;
								data.sound_pitch *= 0.80f;
								data.recoil_multiplier *= 2.00f;
							}
							else
							{
								data.stability -= MathF.Min(data.stability, 0.02f);
								data.stability = MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							}
						}
						break;

						default:
						{
							if (simultaneous)
							{
								data.failure_rate *= 1.70f;
								data.failure_rate += 0.20f;

								data.stability -= MathF.Min(data.stability, 0.30f);
								data.stability = Maths.Clamp(data.stability * 0.70f, 0.00f, 1.00f);

								data.cycle_interval *= 1.30f;
								data.reload_interval *= 1.30f;

								data.jitter_multiplier *= 2.30f;
								data.jitter_multiplier += 3.00f;

								data.sound_volume *= 1.50f;
								data.sound_pitch *= 0.80f;
								data.recoil_multiplier *= 2.00f;
							}
							else
							{
								data.failure_rate *= 3.50f;
								data.failure_rate += 0.05f;

								data.stability -= MathF.Min(data.stability, 0.10f);
								data.stability = Maths.Clamp(data.stability * 0.95f, 0.00f, 1.00f);

								data.reload_interval *= 1.30f;

								data.jitter_multiplier *= 2.50f;
								data.jitter_multiplier += 3.00f;

								data.cycle_interval *= 0.70f;
							}
						}
						break;
					}
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var simultaneous = ref handle.GetData<bool>();

					if (data.feed == Gun.Feed.Single)
					{
						data.max_ammo += 1;
					}

					if (simultaneous)
					{

					}
					else
					{

					}

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier += 0.30f;
					}

					return true;
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					for (int i = 0; i < context.requirements_old.Length; i++)
					{
						var requirement = context.requirements_old[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Metal))
							{
								context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.40f));
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= 0.50f;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= 0.75f;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.70f;
									context.requirements_new.Add(requirement);
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.flared_barrel",
				name: "Flared Barrel",
				description: "Increases spread and loudness, but also greatly reduces recoil.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("##stuff", ref value, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();

					data.damage_multiplier *= Maths.Lerp(0.90f, 0.60f, value);

					data.flash_size *= Maths.Lerp(0.80f, 0.90f, value);
					data.velocity_multiplier *= Maths.Lerp(0.90f, 0.50f, value);

					data.jitter_multiplier *= Maths.Lerp(1.10f, 1.50f, value);
					data.jitter_multiplier += Maths.Lerp(2.50f, 3.00f, value);
					data.sound_volume *= Maths.Lerp(1.15f, 1.35f, value);
					data.sound_size *= Maths.Lerp(1.10f, 1.30f, value);
					data.sound_pitch *= Maths.Lerp(0.98f, 0.96f, value);
					data.recoil_multiplier *= Maths.Lerp(0.70f, 0.20f, value);
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.battering",
				name: "Battering",
				description: "Greatly increases knockback.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.knockback == 0.00f)
					{
						data.knockback += 0.20f;
					}
					data.knockback *= 2.00f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 1.30f;
					}
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 1.30f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.10f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.hooking",
				name: "Hooking",
				description: "Inverts the knockback by adding tiny hooks.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.knockback *= -0.75f;
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 1.10f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.30f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.longer_handle",
				name: "Longer Handle",
				description: "Lengthens the handle of the weapon, allowing you to strike further with more power.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var count = 0;
					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}
					return count < 2;
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.max_distance *= 1.20f;
					data.cooldown *= 1.10f;
					data.damage_base *= 1.15f;
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 1.10f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.uneven_strike",
				name: "Uneven Strike",
				description: "Reduces base damage, but also increases bonus damage.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var count = 0;
					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}
					return count < 2;
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage_bonus += data.damage_base * 0.50f;
					data.damage_base *= 0.80f;
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 1.20f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 0.80f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.reliable_strike",
				name: "Reliable Strike",
				description: "Half of the weapon's bonus damage is converted to base damage",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var count = 0;
					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}
					return count < 2 && data.damage_bonus > 0.00f;
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage_base += data.damage_bonus * 0.50f;
					data.damage_bonus *= 0.50f;
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							//requirement.amount *= 1.10f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.20f;
							requirement.difficulty += 1.00f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.hollowed",
				name: "Hollowed",
				description: "Hollows out parts of the weapon, increasing swing speed, while also reducing damage, knockback and cost.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var count = 0;
					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}
					return count < 2;
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage_base *= 0.80f;
					data.damage_bonus *= 0.90f;
					data.knockback *= 0.30f;
					data.cooldown *= 0.75f;
					// TODO: This should reduce the weight as well

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 0.70f;
					}
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 0.70f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.70f;
							requirement.difficulty += 1.00f;
							if (requirement.work == Work.Type.Smithing)
							{
								requirement.difficulty += 3.00f;
								requirement.amount += 100.00f;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.resourceful_sharpening",
				name: "Resourceful Sharpening",
				description: "Sharpens the tool to render less of the struck material unusable, increasing resource yield.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.yield > 0.00f && data.yield < 1.50f && data.damage_type == Damage.Type.Slash;
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage_base *= 0.90f;
					data.damage_bonus *= 0.90f;
					data.yield = Maths.Clamp(data.yield + 0.10f, 0.00f, 1.00f);
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.20f;
							requirement.difficulty += 3.00f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.toy_weapon",
				name: "Toy Weapon",
				description: "Dulls the weapon to deal no damage.", // Currently this also reduces kockback to 0 cause knockback is bad and based on damage

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.damage_base > 0.00f || data.damage_bonus > 0.00f;
				},

				finalize: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage_base = 0.00f;
					data.damage_bonus = 0.00f;

					return true;
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 0.70f;
						}
					}
				}
			));

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

			definitions.Add(Modification.Definition.New<Medkit.Data>
			(
				identifier: "medkit.alcohol",
				category: "Medkit",
				name: "Alcohol Disinfectant",
				description: "Adds an alcohol disinfectant, increasing healing amount at cost of extra pain.",

				can_add: static (ref Modification.Context context, in Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.pain += 250.00f;
					data.power *= 2.50f;
					context.requirements_new.Add(Crafting.Requirement.Resource("alcohol", 5));
				}
			));

			definitions.Add(Modification.Definition.New<Medkit.Data>
			(
				identifier: "medkit.extra_bandages",
				category: "Medkit",
				name: "Extra Bandages",
				description: "Increases area of effect.",

				can_add: static (ref Modification.Context context, in Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.aoe *= 1.50f;
					data.max_distance += 0.50f;
					context.requirements_new.Add(Crafting.Requirement.Resource("cloth", 8));
				}
			));


			definitions.Add(Modification.Definition.New<Medkit.Data>
			(
				identifier: "medkit.sterile",
				category: "Medkit",
				name: "Sterile Surgical Tools",
				description: "Allows treating more severe injuries.",

				can_add: static (ref Modification.Context context, in Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.heal_cap_primary += 0.15f;
					data.heal_cap_secondary += 0.10f;

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

			//definitions.Add(Modification.Definition.New<Medkit.Data>
			//(
			//	identifier: "medkit.replacement_skin",
			//	category: "Medkit",
			//	name: "Replacement Skin",
			//	description: "Graft skin and flesh (Very painfull)",

			//	can_add: static (ref Modification.Context context, in Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		return !modifications.HasModification(handle);
			//	},

			//	apply_1: static (ref Modification.Context context, ref Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		data.cooldown *= 1.20f;
			//		data.power *= 1.70f;
			//		data.heal_cap = MathF.Min(1.00f, data.heal_cap + 0.05f);
			//		data.pain += 300.00f;
			//		context.requirements_new.Add(Crafting.Requirement.Resource("meat", 150));
			//	}
			//));

			//definitions.Add(Modification.Definition.New<Medkit.Data>
			//(
			//	identifier: "medkit.mithril_treatment",
			//	category: "Medkit",
			//	name: "Experimental Mithril Treatment",
			//	description: "Mithril does wonders to the flesh, though the reaction is a bit strange.",

			//	can_add: static (ref Modification.Context context, in Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		return !modifications.HasModification(handle);
			//	},

			//	apply_1: static (ref Modification.Context context, ref Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		data.power *= 1.50f; // Power is justified due to mithril being hideously expensive
			//		data.pain += 100.00f;

			//		data.critical_heal += 3.00f; // Quadruple healing at 0 hp, 2.5x healing at 50% hp
			//		data.heal_cap_primary += 0.05f;
			//		data.heal_cap_secondary -= 0.20f;

			//		context.requirements_new.Add(Crafting.Requirement.Resource("mithril", 3));
			//	}
			//));

			//definitions.Add(Modification.Definition.New<Medkit.Data>
			//(
			//	identifier: "medkit.blue_mushrooms",
			//	category: "Medkit",
			//	name: "Blue Mushroom Treatment",
			//	description: "Blue Mushroom",

			//	can_add: static (ref Modification.Context context, in Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		return !modifications.HasModification(handle);
			//	},

			//	apply_1: static (ref Modification.Context context, ref Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		data.power *= 1.20f;
			//		data.cooldown *= 0.70f;
			//		data.pain -= 100.00f;

			//		data.critical_heal -= 2.00f; // No healing at half hp or lower

			//		context.requirements_new.Add(Crafting.Requirement.Resource("mushroom.blue", 100));
			//	}
			//));

			definitions.Add(Modification.Definition.New<Medkit.Data>
			(
				identifier: "medkit.red_sugar",
				category: "Medkit",
				name: "Red Sugar Anesthetic",
				description: "Red sugar numbs the pain with sweetness.",

				can_add: static (ref Modification.Context context, in Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Medkit.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.pain -= 200.00f;
					context.requirements_new.Add(Crafting.Requirement.Resource("red_sugar", 10));
				}
			));
		}
	}
}

