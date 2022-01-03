using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterGunModifications(ref List<Modification.Definition> definitions)
		{
			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.expanded_magazine",
				name: "Expanded Magazine",
				description: "Increases ammo capacity.",

				validate: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 1.00f, 5.00f);

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
					if (data.feed == Gun.Feed.Single || data.feed == Gun.Feed.Breech || data.feed == Gun.Feed.Front) return false;
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

				validate: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.00f, 1.00f);

					return true;
				},

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
					var ratio = value;

					switch (data.type)
					{
						case Gun.Type.Shotgun:
						{
							data.velocity_multiplier *= Maths.Lerp(1.00f, 0.80f, ratio);
							data.damage_multiplier *= Maths.Lerp(1.00f, 0.85f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.60f, ratio);
						}
						break;

						case Gun.Type.Rifle:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.55f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.22f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.50f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.20f, ratio);
						}
						break;

						case Gun.Type.MachineGun:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.60f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.24f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.50f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.20f, ratio);
						}
						break;

						case Gun.Type.SMG:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.50f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.20f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.60f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.30f, ratio);
						}
						break;

						case Gun.Type.Handgun:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.43f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.15f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.65f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.10f, ratio);
						}
						break;

						default:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.30f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.15f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.50f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.15f, ratio);
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
					ref var value = ref handle.GetData<float>();
					var ratio = value;

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.difficulty *= Maths.Lerp(1.00f, 1.75f, ratio);
									requirement.amount += Maths.Lerp(0.00f, 350.00f, ratio);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.difficulty *= Maths.Lerp(1.00f, 1.50f, ratio);
									requirement.amount += Maths.Lerp(0.00f, 200.00f, ratio);
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

			//definitions.Add(Modification.Definition.New<Gun.Data>
			//(
			//	identifier: "gun.caliber_downgrade",
			//	name: "Caliber Downgrade",
			//	description: "Rechambers to a lower caliber.",

			//	can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		return !modifications.HasModification(handle) && data.ammo_filter.HasAny(Material.Flags.Ammo_AC | Material.Flags.Ammo_MG | Material.Flags.Ammo_HC);
			//	},

			//	apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
			//	{
			//		if (data.ammo_filter.HasAll(Material.Flags.Ammo_AC))
			//		{
			//			data.ammo_filter |= Material.Flags.Ammo_MG;
			//			data.ammo_filter &= ~Material.Flags.Ammo_AC;

			//			data.damage_multiplier *= 1.50f;
			//			data.velocity_multiplier *= 1.10f;
			//			data.failure_rate *= 0.20f;
			//			data.failure_rate += 0.10f;
			//			data.recoil_multiplier *= 0.50f;
			//			data.jitter_multiplier += 1.50f;
			//			data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.80f));

			//			data.stability = Maths.Clamp(data.stability * 3.50f, 0.00f, 1.00f);

			//			data.sound_pitch *= 1.15f;
			//			data.sound_volume *= 1.02f;

			//			data.smoke_size *= 0.90f;
			//			data.flash_size *= 0.90f;
			//		}
			//		else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
			//		{
			//			data.ammo_filter |= Material.Flags.Ammo_HC;
			//			data.ammo_filter &= ~Material.Flags.Ammo_MG;

			//			data.damage_multiplier *= 1.35f;
			//			data.velocity_multiplier *= 0.95f;
			//			data.failure_rate *= 1.90f;
			//			data.failure_rate += 0.50f;
			//			data.recoil_multiplier *= 0.70f;
			//			data.jitter_multiplier += 0.50f;
			//			data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.30f));

			//			data.stability = Maths.Clamp(data.stability * 1.20f, 0.00f, 1.00f);

			//			data.sound_pitch *= 1.15f;
			//			data.sound_volume *= 1.02f;

			//			data.smoke_size *= 0.90f;
			//			data.flash_size *= 0.90f;
			//		}
			//		else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
			//		{
			//			data.ammo_filter |= Material.Flags.Ammo_LC;
			//			data.ammo_filter &= ~Material.Flags.Ammo_HC;

			//			data.damage_multiplier *= 1.20f;
			//			data.velocity_multiplier *= 0.85f;
			//			data.failure_rate *= 1.70f;
			//			data.failure_rate += 0.50f;
			//			data.recoil_multiplier *= 0.60f;
			//			data.jitter_multiplier += 0.50f;
			//			data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.20f));

			//			data.stability = Maths.Clamp(data.stability * 1.40f, 0.00f, 1.00f);

			//			data.sound_pitch *= 1.10f;
			//			data.sound_volume *= 1.02f;

			//			data.smoke_size *= 0.90f;
			//			data.flash_size *= 0.90f;
			//		}
			//	}
			//));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.caliber_conversion",
				name: "Caliber Conversion",
				description: "Rechambers to a different caliber.",

				validate: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<int>();
					value = Maths.Clamp(value, -2, 2); // TODO: Make this clamped between available calibers
					
					return true;
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<int>();
					return GUI.SliderInt("##stuff", ref value, -2, 2, "%d");
				},
#endif

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle) && data.ammo_filter.HasAny(Material.Flags.Ammo_LC | Material.Flags.Ammo_HC | Material.Flags.Ammo_MG | Material.Flags.Ammo_AC);
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<int>();

					var count = Math.Abs(value);
					if (value > 0)
					{
						for (int i = 0; i < count; i++)
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
					}
					else if (value < 0)
					{
						for (int i = 0; i < count; i++)
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
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.recoil_reduction",
				name: "Recoil Reduction",
				description: "Reduces recoil.",

				validate: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.00f, 1.00f);

					return true;
				},

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

				// TODO: lerp the values based on ratio
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

				validate: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 1.00f, 2.00f);

					return true;
				},

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
						health.armor += 70.00f;
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

				validate: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.00f, 1.00f);

					return true;
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
				identifier: "gun.revolver_gas_seal", // Should allow the use of silencer attachment on revolver in the future
				name: "Gas seal",
				description: "Increases damage and velocity, by moving cylinder closer to barrel before each shot to avoid flash gap in revolver.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.feed != Gun.Feed.Cylinder) return false;
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.85f;
					data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
					data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
					data.damage_multiplier *= 1.10f;
					data.velocity_multiplier *= 1.05f;
					data.recoil_multiplier *= 1.10f;
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.60f;
					data.jitter_multiplier *= 0.90f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 1.05f;
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
								requirement.amount *= 1.10f;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 1.50f;
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 1.05f;
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 2.00f;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 2.00f;
								}
								break;
							}
						}
					}
				}
			));
						
			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.revolver_hand_fitted_parts",
				name: "Hand fitted parts",
				description: "Increases damage and velocity, by making flash gap in revolver shorter due to very careful construction, at the cost of workspeed.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.feed != Gun.Feed.Cylinder) return false;
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.85f;
					data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
					data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
					data.damage_multiplier *= 1.12f;
					data.velocity_multiplier *= 1.06f;
					data.recoil_multiplier *= 1.12f;
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.60f;
					data.jitter_multiplier *= 0.90f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 1.05f;
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
								requirement.amount *= 1.10f;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 2.50f;
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 1.05f;
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 3.00f;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 3.00f;
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.revolver_side_gate_loading",
				name: "Side gate loading",
				description: "Simplifies the revolver construction, increasing reliability at the cost of reload speed.",

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.feed != Gun.Feed.Cylinder) return false;
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.85f;
					data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
					data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
					data.reload_interval *= 2.00f;
				},

				finalize: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.60f;
					data.jitter_multiplier *= 0.90f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 0.95f;
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
								requirement.amount *= 0.90f;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 0.60f;
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 0.95f;
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 0.60f;
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

				validate: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.00f, 1.00f);

					return true;
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

						case Gun.Action.Manual:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.80f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.75f, ratio);
						}
						break;

						case Gun.Action.Lever:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.80f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.75f, ratio);
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
				identifier: "gun.slower_cycling_mechanism",
				name: "Slower Cycling Mechanism",
				description: "Decreases fire rate, but increases reliability.",

				validate: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 1.00f, 2.00f);

					return true;
				},

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
							var mult = 1.00f - ratio;

							data.failure_rate *= 0.85f;
							data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
							data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							data.cycle_interval *= Maths.Lerp(1.20f, 10.00f, ratio);
						}
						break;

						case Gun.Type.Rifle:
						{
							var mult = 1.00f - ratio;

							data.failure_rate *= 0.85f;
							data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
							data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							data.cycle_interval *= Maths.Lerp(1.20f, 5.00f, ratio);
						}
						break;

						case Gun.Type.Shotgun:
						{
							data.failure_rate *= 0.85f;
							data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
							data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							data.cycle_interval *= Maths.Lerp(1.20f, 5.00f, ratio);
						}
						break;

						case Gun.Type.SMG:
						{
							data.failure_rate *= 0.85f;
							data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
							data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							data.cycle_interval *= Maths.Lerp(1.20f, 10.00f, ratio);
						}
						break;

						default:
						{
							data.failure_rate *= 0.85f;
							data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
							data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							data.cycle_interval *= Maths.Lerp(1.20f, 10.00f, ratio);
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
						}
						break;
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
					return data.feed != Gun.Feed.Single && data.feed != Gun.Feed.Breech && data.feed != Gun.Feed.Front && !modifications.HasModification(handle);
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
						case Gun.Feed.Breech:
						case Gun.Feed.Front:
						case Gun.Feed.Single:
						{
							if (simultaneous)
							{
								data.failure_rate *= 1.10f;
								data.failure_rate += 0.25f;

								data.stability -= MathF.Min(data.stability, 0.50f);
								data.stability = Maths.Clamp(data.stability * 0.60f, 0.00f, 1.00f);

								//data.reload_interval *= 1.30f;

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

					if (data.feed == Gun.Feed.Single || data.feed == Gun.Feed.Breech || data.feed == Gun.Feed.Front)
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
					ref var overheat = ref context.GetComponent<Overheat.Data>();
					if (!overheat.IsNull())
					{
						overheat.cool_rate *= 1.30f;
					}

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

				validate: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.00f, 1.00f);

					return true;
				},

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

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.automatic_reloading",
				category: "Gun",
				name: "Automatic Reloading",
				description: "Automatically reloads the weapon once the magazine is empty.",

				// This is not extremely useful, but saves you from pressing a button and works nicely for mounted items

				can_add: static (ref Modification.Context context, in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var automatic_reload = ref context.GetOrAddComponent<AutomaticReload.Data>();
				},

				apply_1: static (ref Modification.Context context, ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 5.00f));
					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100.00f, 20));
				}
			));
		}
	}
}

