﻿using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterModifications(ref List<Modification.Definition> definitions)
		{
			definitions.Add(Modification.Definition.New<Fuse.Data>
			(
				identifier: "fuse.length",
				name: "Fuse Length",
				description: "Modifies fuse's length.",

				can_add: static (in Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var count = 0;
					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}
					return count < 1;
				},

#if CLIENT
				draw_editor: static (in Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.DragFloat("##stuff", ref value, 0.10f, 0.50f, 10.00f, "%.2f");
				},
#endif

				apply: static (ref Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var value = MathF.Max(0.50f, handle.GetData<float>());
					data.time = value;
				}
			));

			definitions.Add(Modification.Definition.New<Fuse.Data>
			(
				identifier: "fuse.inextinguishable",
				name: "Inextinguishable Fuse",
				description: "Makes the fuse impossible to be extinguished.",

				can_add: static (in Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var count = 0;
					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}
					return count < 1;
				},

				apply: static (ref Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_chance = 0.00f;
				}
			));

			definitions.Add(Modification.Definition.New<Explosive.Data>
			(
				identifier: "explosive.directed",
				name: "Directed Explosion",
				description: "Focuses the explosion into a smaller area.",

				apply: static (ref Explosive.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					//var radius_log = MathF.Log2(data.radius);

					data.radius *= 0.75f;
					data.power *= 1.25f;
					data.damage_terrain *= 1.25f;
					data.damage_entity *= 1.25f;
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.expanded_magazine",
				name: "Expanded Magazine",
				description: "Increases ammo capacity.",

				can_add: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.feed != Gun.Feed.Single;
				},

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var amount = 0.00f;

					switch (data.feed)
					{
						case Gun.Feed.Drum:
						{
							amount += 10;
						}
						break;

						case Gun.Feed.Cylinder:
						{
							amount += 1;
						}
						break;

						case Gun.Feed.Clip:
						{
							amount += 2;
						}
						break;

						case Gun.Feed.Magazine:
						{
							amount += 5;
						}
						break;
					}

					switch (data.type)
					{
						case Gun.Type.AutoCannon:
						{
							amount *= 1.50f;
						}
						break;

						case Gun.Type.MachineGun:
						{
							amount *= 1.50f;
						}
						break;
					}

					data.max_ammo += amount;
				},

				requirements: static (Span<Crafting.Requirement> requirements, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in requirements)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (!material.flags.HasFlag(Material.Flags.Manufactured) && material.type == Material.Type.Metal)
							{
								requirement.amount *= 1.30f;
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
				draw_editor: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.DragFloat("##stuff", ref value, 0.01f, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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

				requirements: static (Span<Crafting.Requirement> requirements, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in requirements)
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

				can_add: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.action == Gun.Action.Gas || data.action == Gun.Action.Blowback || data.action == Gun.Action.Crank;
				},

				finalize: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (!data.flags.HasFlag(Gun.Flags.Automatic))
					{
						switch (data.action)
						{
							case Gun.Action.Gas:
							case Gun.Action.Blowback:
							case Gun.Action.Crank:
							{
								data.flags |= Gun.Flags.Automatic;

								data.cycle_interval *= 1.20f;

								data.failure_rate *= 3.50f;
								data.failure_rate += 0.02f;

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

				can_add: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.action == Gun.Action.Gas || data.action == Gun.Action.Blowback || data.action == Gun.Action.Crank;
				},

				finalize: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.flags.HasFlag(Gun.Flags.Automatic))
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

				can_add: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.ammo_filter.HasFlag(Material.Flags.Ammo_AC) || data.ammo_filter.HasFlag(Material.Flags.Ammo_MG) || data.ammo_filter.HasFlag(Material.Flags.Ammo_HC);
				},

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.ammo_filter.HasFlag(Material.Flags.Ammo_AC))
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

						data.stability *= 3.50f;

						data.sound_pitch *= 1.15f;
						data.sound_volume *= 1.02f;

						data.smoke_size *= 0.90f;
						data.flash_size *= 0.90f;
					}
					else if (data.ammo_filter.HasFlag(Material.Flags.Ammo_MG))
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

						data.stability *= 1.20f;

						data.sound_pitch *= 1.15f;
						data.sound_volume *= 1.02f;

						data.smoke_size *= 0.90f;
						data.flash_size *= 0.90f;
					}
					else if (data.ammo_filter.HasFlag(Material.Flags.Ammo_HC))
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

						data.stability *= 1.40f;

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

				can_add: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.ammo_filter.HasFlag(Material.Flags.Ammo_LC) || data.ammo_filter.HasFlag(Material.Flags.Ammo_HC) || data.ammo_filter.HasFlag(Material.Flags.Ammo_MG);
				},

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.ammo_filter.HasFlag(Material.Flags.Ammo_LC))
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
						data.stability *= 0.80f;

						data.sound_pitch *= 0.82f;
						data.sound_volume *= 1.20f;

						data.smoke_size *= 1.30f;
						data.flash_size *= 1.10f;
					}
					else if (data.ammo_filter.HasFlag(Material.Flags.Ammo_HC))
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
						data.stability *= 0.80f;

						data.sound_pitch *= 0.87f;
						data.sound_volume *= 1.25f;

						data.smoke_size *= 1.30f;
						data.flash_size *= 1.10f;
					}
					else if (data.ammo_filter.HasFlag(Material.Flags.Ammo_MG))
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
						data.stability *= 0.70f;

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
				draw_editor: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.DragFloat("##stuff", ref value, 0.01f, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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
							data.stability *= 1.09f;

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
							data.stability *= 1.07f;

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

				can_add: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var count = 0;

					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}

					return count < 1;
				},

#if CLIENT
				draw_editor: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.DragFloat("##stuff", ref value, 0.01f, 1.00f, 2.00f, "%.2f");
				},
#endif

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.unstable_receiver",
				name: "Unstable Receiver",
				description: "Greatly improves most of the gun's properties, at cost of reduced stability and reliability.",

#if CLIENT
				draw_editor: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.DragFloat("##stuff", ref value, 0.01f, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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

					if (data.flags.HasFlag(Gun.Flags.Automatic))
					{
						data.failure_rate = Maths.Clamp(data.failure_rate * 1.50f, 0.00f, 1.00f);
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.hardened_frame",
				name: "Hardened Frame",
				description: "Improves reliability of the gun.",

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.50f;
					data.stability += 0.20f;
					data.stability = Maths.Clamp(data.stability * 1.30f, 0.00f, 1.00f);
				},

				finalize: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate -= MathF.Min(data.failure_rate, 0.20f);
					data.failure_rate *= 0.70f;
					data.stability += 0.10f;
					data.stability = Maths.Clamp(data.stability * 1.10f, 0.00f, 1.00f);

					return true;
				},

				requirements: static (Span<Crafting.Requirement> requirements, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in requirements)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (!material.flags.HasFlag(Material.Flags.Manufactured))
							{
								requirement.amount *= 1.75f;
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
				draw_editor: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.DragFloat("##stuff", ref value, 0.01f, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();

					data.failure_rate *= Maths.Lerp(1.20f, 1.80f, value);
					data.failure_rate += Maths.Lerp(0.03f, 0.10f, value);
					data.stability *= Maths.Clamp(data.stability, 0.00f, 0.99f);
					data.stability -= 0.03f;
					data.cycle_interval *= 1.05f;
					data.reload_interval *= 1.10f;
				},

				finalize: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();

					data.failure_rate += 0.03f;
					data.stability *= Maths.Clamp(data.stability, 0.00f, Maths.Lerp(0.98f, 0.95f, value));

					return true;
				},

				requirements: static (Span<Crafting.Requirement> requirements, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();

					foreach (ref var requirement in requirements)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (!material.flags.HasFlag(Material.Flags.Manufactured))
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

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.85f;
					data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
					data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
					data.cycle_interval *= 1.35f;
					data.reload_interval *= 0.93f;
					data.damage_multiplier *= 0.98f;
				},

				finalize: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate *= 0.60f;
					data.jitter_multiplier += 1.50f;

					return true;
				},

				requirements: static (Span<Crafting.Requirement> requirements, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in requirements)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasFlag(Material.Flags.Manufactured))
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
				draw_editor: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.DragFloat("##stuff", ref value, 0.01f, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					var ratio = value;

					switch (data.action)
					{
						case Gun.Action.Gas:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 4.50f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.40f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.35f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 1.40f, ratio);
						}
						break;

						case Gun.Action.Blowback:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 2.20f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.03f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.55f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 1.40f, ratio);
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

					//if (data.flags.HasFlag(Gun.Flags.Automatic))
					//{
					//	data.failure_rate *= Maths.Lerp(1.00f, 2.50f, ratio);
					//}

					data.stability -= MathF.Min(data.stability, Maths.Lerp(0.00f, (1.00f / data.cycle_interval) * 0.15f, ratio));
					data.failure_rate += Maths.Lerp(0.00f, (1.00f / data.cycle_interval) * 0.35f, ratio);
				},

				requirements: static (Span<Crafting.Requirement> requirements, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in requirements)
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

				can_add: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.action == Gun.Action.Blowback;
				},

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					switch (data.action)
					{
						case Gun.Action.Blowback:
						{
							data.damage_multiplier *= 1.25f;

							data.failure_rate *= 0.70f;
							data.failure_rate = MathF.Max(0.00f, data.failure_rate - 0.03f);
							data.cycle_interval *= 0.90f;

							data.stability += MathF.Min(data.stability, 0.15f);
							data.stability *= 1.20f;

							data.action = Gun.Action.Gas;
						}
						break;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.single_shot",
				name: "Feed: Single-Shot",
				description: "Converts to single-shot feed mechanism.",

				can_add: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var count = 0;

					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}

					return count < 1 && data.feed != Gun.Feed.Single;
				},

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.feed = Gun.Feed.Single;
					data.max_ammo = 1;
				},

				finalize: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_rate = Maths.Clamp(data.failure_rate * 0.10f + (1.00f - Maths.Clamp(data.stability, 0.00f, 1.00f)), 0.00f, 1.00f);

					return true;
				}
			));

			definitions.Add(Modification.Definition.New<Gun.Data>
			(
				identifier: "gun.extra_barrel",
				name: "Extra Barrel",
				description: "Adds an extra barrel to the gun.",

#if CLIENT
				draw_editor: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var simultaneous = ref handle.GetData<bool>();
					return GUI.Checkbox("Simultaneous Fire", ref simultaneous, GUI.GetRemainingSpace());
				},
#endif

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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
								data.stability *= 0.60f;

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
								data.stability *= 0.70f;

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
								data.stability *= 0.95f;

								data.reload_interval *= 1.30f;

								data.jitter_multiplier *= 2.50f;
								data.jitter_multiplier += 3.00f;

								data.cycle_interval *= 0.70f;
							}
						}
						break;
					}
				},

				finalize: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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

					return true;
				},

				requirements: static (Span<Crafting.Requirement> requirements, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in requirements)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount += 100.00f;
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount += 80.00f;
								}
								break;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasFlag(Material.Flags.Metal))
							{
								requirement.amount *= 1.50f;
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

#if CLIENT
				draw_editor: static (in Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.DragFloat("##stuff", ref value, 0.01f, 0.00f, 1.00f, "%.2f");
				},
#endif

				apply: static (ref Gun.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();

					data.flash_size *= Maths.Lerp(0.80f, 0.90f, value);
					data.velocity_multiplier *= Maths.Lerp(0.90f, 0.50f, value);

					data.jitter_multiplier *= Maths.Lerp(1.10f, 1.50f, value);
					data.jitter_multiplier += Maths.Lerp(2.50f, 3.00f, value);
					data.sound_volume *= Maths.Lerp(1.45f, 1.55f, value);
					data.sound_size *= Maths.Lerp(1.50f, 1.60f, value);
					data.sound_pitch *= Maths.Lerp(0.90f, 0.85f, value);
					data.recoil_multiplier *= Maths.Lerp(0.70f, 0.20f, value);
				}
			));
		}
	}
}

