
namespace TC2.Base.Components
{
	public static partial class OrganicExt
	{
		[ISystem.Update(ISystem.Mode.Single, order: 5, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit)]
		public static void UpdateBrain([Source.Owned, Original] ref Organic.Data organic_original, [Source.Owned, Override] in Organic.Data organic_override, [Source.Owned] ref Organic.State organic_state, [Source.Owned] in Health.Data health, [Source.Owned] bool dead)
		{
			if (organic_original.tags.HasAll(Organic.Tags.Brain))
			{
				var p = (MathF.Pow(MathF.Max(0.00f, organic_state.pain_shared - 100.00f) * 0.002f, 1.20f) * 0.12f);
				//organic_original.consciousness = Maths.Lerp(organic_original.consciousness, 1.00f - Maths.Clamp01(p), 0.02f); // player.flags.HasAll(Player.Flags.Alive) ? 1.00f : 0.30f;
				organic_original.consciousness = Maths.Lerp2(organic_original.consciousness, MathF.Min(1.00f - Maths.Clamp01(p), 1.00f - (organic_state.stun_norm * 0.60f)), 0.10f, 0.02f); // player.flags.HasAll(Player.Flags.Alive) ? 1.00f : 0.30f;

				var health_norm = health.GetHealthNormalized();
				if (dead || health_norm < 0.50f)
				{
					organic_original.consciousness = 0.00f;
					organic_state.unconscious_time = 20.00f;

					organic_state.consciousness_shared = 0.00f;
					organic_state.consciousness_shared_new = 0.00f;

					organic_state.motorics_shared = 0.00f;
					organic_state.motorics_shared_new = 0.00f;
				}
				else
				{
					//App.WriteLine(p);
					organic_state.consciousness_shared_new = organic_override.consciousness; // Maths.Clamp01(consciousness - ((1.00f - health_norm) * 0.20f) - p); // Maths.Clamp((consciousness - ( (MathF.Pow(organic_state.pain_shared * 0.002f, 1.20f) * 0.12f) * ) ), 0.00f, 1.00f);
					organic_state.motorics_shared_new = MathF.Max(organic_state.consciousness_shared_new - Maths.Clamp(p, 0.00f, 0.35f), 0.00f) * (1.00f - organic_state.stun_norm);
				}
			}
		}

		[ISystem.Update(ISystem.Mode.Single, order: 10, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit), HasComponent<Head.Data>(Source.Modifier.Owned, false)]
		public static void UpdateConnected(Entity ent_organic_parent, Entity ent_organic_child,
		[Source.Parent, Override] in Organic.Data organic_parent, [Source.Parent] ref Organic.State organic_state_parent,
		[Source.Owned, Override] in Organic.Data organic_child, [Source.Owned] ref Organic.State organic_state_child,
		[Source.Parent] in Joint.Base joint)
		{
			if (joint.flags.HasAll(Joint.Flags.Organic))
			{
				organic_state_child.consciousness_shared_new = MathF.Max(organic_state_child.consciousness_shared_new, organic_state_parent.consciousness_shared);
				organic_state_child.motorics_shared_new = MathF.Max(organic_state_child.motorics_shared_new, organic_state_parent.motorics_shared);
				organic_state_child.unconscious_time = organic_state_parent.unconscious_time;

				organic_state_parent.pain_shared_new = organic_state_child.pain_shared_new = MathF.Max(organic_state_parent.pain_shared_new, organic_state_child.pain_shared_new);
			}
		}

		// TODO: Shitcoded workaround so head always updates after other body parts (otherwise it won't affect consciousness, in case the system runs on the head first)
		[ISystem.Update(ISystem.Mode.Single, order: 15, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit), HasComponent<Head.Data>(Source.Modifier.Owned, true)]
		public static void UpdateConnectedHead(Entity ent_organic_parent, Entity ent_organic_child,
		[Source.Parent, Override] in Organic.Data organic_parent, [Source.Parent] ref Organic.State organic_state_parent,
		[Source.Owned, Override] in Organic.Data organic_child, [Source.Owned] ref Organic.State organic_state_child,
		[Source.Parent] in Joint.Base joint)
		{
			if (joint.flags.HasAll(Joint.Flags.Organic))
			{
				organic_state_parent.consciousness_shared_new = organic_state_child.consciousness_shared;
				organic_state_parent.motorics_shared_new = organic_state_child.motorics_shared;
				organic_state_parent.unconscious_time = organic_state_child.unconscious_time;

				organic_state_parent.pain_shared_new = organic_state_child.pain_shared_new = MathF.Max(organic_state_parent.pain_shared_new, organic_state_child.pain_shared_new);
			}
		}


		[ISystem.VeryLateUpdate(ISystem.Mode.Single, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit)]
		public static void Update2(ISystem.Info info, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state, [Source.Owned] in Health.Data health, [Source.Owned] bool dead)
		{
			organic_state.consciousness_shared = Maths.Lerp(organic_state.consciousness_shared, organic_state.consciousness_shared_new, 0.20f);
			organic_state.motorics_shared = Maths.Lerp(organic_state.motorics_shared, organic_state.motorics_shared_new, 0.20f);
			organic_state.pain_shared = Maths.Lerp(organic_state.pain_shared, organic_state.pain_shared_new * organic.pain_modifier, 0.20f);
			organic_state.pain = Maths.Lerp(organic_state.pain, organic_state.pain * (0.15f + (MathF.Max(0.00f, 0.60f - MathF.Pow(health.GetHealthNormalized(), 6.00f) * 0.90f))).Clamp01() * organic.pain_modifier, 0.008f);
			organic_state.stun = Maths.MoveTowards(organic_state.stun, 0.00f, info.DeltaTime * 50.00f);

			organic_state.stun_norm = Maths.NormalizeClamp(organic_state.stun, 500.00f).Pow2();
			organic_state.efficiency = organic_state.motorics_shared.Clamp01() * health.GetHealthNormalized();
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit)]
		public static void UpdateConsciousness(ISystem.Info info, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			if (organic_state.consciousness_shared > 0.20f)
			{
				organic_state.unconscious_time = 0.00f;
			}
			else
			{
				organic_state.unconscious_time += App.fixed_update_interval_s * ((1.00f - Maths.NormalizeClamp(organic_state.consciousness_shared, 0.50f)) * 5.00f);
			}
		}

		[ISystem.Update(ISystem.Mode.Single, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit)]
		public static void UpdateJoint([Source.Shared] in Organic.State organic_state, [Source.Owned] ref Joint.Base joint)
		{
			if (joint.flags.HasAll(Joint.Flags.Organic))
			{
				var modifier = Maths.Lerp01(organic_state.efficiency * organic_state.efficiency, organic_state.consciousness_shared * 1.50f, 0.50f) * (1.00f - organic_state.stun_norm);
				joint.torque_modifier = modifier;
			}
		}

		[ISystem.Update(ISystem.Mode.Single, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit), HasTag("dead", true, Source.Modifier.Owned), HasComponent<Organic.Data>(Source.Modifier.Owned, true)]
		public static void UpdateNoRotate([Source.Owned, Override] ref NoRotate.Data no_rotate)
		{
			no_rotate.multiplier = 0.00f;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit)]
		public static void UpdateArm<T>([Source.Shared, Override] in Organic.Data organic, [Source.Shared] ref Organic.State organic_state, [Source.Owned] in Arm.Data arm, [Source.Owned, Override] ref T joint) where T : unmanaged, IComponent, IRotaryJoint
		{
			joint.MaxSpeed *= Maths.Clamp(organic.motorics, 0.30f, 1.50f);
			joint.MaxBias += (1.00f - organic.motorics.Clamp01()) * 0.15f;
		}

		// TODO: this is bad
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateArmAimable([Source.Parent, Override] in Organic.Data organic, [Source.Parent] ref Arm.Data arm, [Source.Parent, Override] ref Joint.Gear joint_gear,
		[Source.Owned] in Aimable.Data aimable,
		[Source.Parent] in Body.Data body_parent, [Source.Owned] in Body.Data body_child)
		{
			var aim_torque = arm.aim_torque;
			if (arm.aim_torque > 1.00f)
			{
				aim_torque += (body_parent.GetMass() / (arm.aim_torque * 0.000018f)) * organic.strength;
			}

			//App.WriteLine(aim_torque);

			joint_gear.min = MathF.Max(joint_gear.min, aimable.min);
			joint_gear.max = MathF.Min(joint_gear.max, aimable.max);
			joint_gear.step = Maths.Clamp(body_child.GetAngularMassInv() * aim_torque * App.fixed_update_interval_s, 0.50f, joint_gear.step);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit)]
		public static void UpdateFacing([Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state, [Source.Owned] ref Facing.Data facing)
		{
			facing.flags.SetFlag(Facing.Flags.Disabled, organic_state.consciousness_shared < 0.50f || organic_state.stun_norm >= 0.50f);
		}

		[ISystem.Add(ISystem.Mode.Single), HasTag("dead", true, Source.Modifier.Owned)]
		public static void OnAddBodyDead(Entity entity, [Source.Owned, Original] in Organic.Data organic, [Source.Owned] ref Body.Data body)
		{
			//App.WriteLine("add body layer dead");

			body.override_shape_layer.SetFlag(Physics.Layer.Dead, true);
			body.flags.SetFlag(Body.Flags.NonDirty, true);

			//App.WriteLine($"rip {torque}");
			//body.AddTorque(torque);

			var random = XorRandom.New(entity);
			var torque = body.GetAngularMass() * random.NextFloatRange(20.00f, 25.00f).WithSign(random.NextBool(0.50f)) * App.tickrate;
			body.AddTorque(torque);
			body.AddForceWorld(random.NextUnitVector2Range(0.50f, 0.90f) * body.GetMass() * App.tickrate * 10.00f, body.GetPosition() + random.NextUnitVector2Range(0.15f, 0.20f));
		}

		[ISystem.Remove(ISystem.Mode.Single), HasTag("dead", true, Source.Modifier.Owned)]
		public static void OnRemoveBodyDead([Source.Owned, Original] in Organic.Data organic, [Source.Owned] ref Body.Data body)
		{
			//App.WriteLine("remove body layer dead");

			body.override_shape_layer.SetFlag(Physics.Layer.Dead, false);
			body.flags.SetFlag(Body.Flags.NonDirty, true);
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, interval: 0.50f, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit), HasTag("dead", true, Source.Modifier.Owned)]
		public static void UpdateRotting(ISystem.Info info, Entity entity, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			organic_state.rotten = MathF.Max(organic_state.rotten + (info.DeltaTime * Constants.Organic.rotting_speed), 0.00f);

#if SERVER
			//App.WriteLine($"{organic_state.rotten} {entity}");

			if (organic_state.rotten >= 1.00f)
			{
				entity.Delete();
			}
#endif
		}

#if SERVER
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, interval: 0.20f, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit)]
		public static void UpdateDead(Entity entity, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state, [Source.Owned] bool dead)
		{
			if (dead)
			{
				//if (organic_state.unconscious_time < 10.00f) entity.SetTag("dead", false);
			}
			else
			{
				if (organic_state.unconscious_time > 15.00f) entity.SetTag("dead", true);
			}
		}
#endif

		[ISystem.Update(ISystem.Mode.Single, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit)]
		public static void UpdateJoint1A(ISystem.Info info, [Source.Shared] in Transform.Data transform, [Source.Shared] in Health.Data health, [Source.Shared, Override] in Organic.Data organic, [Source.Shared] ref Organic.State organic_state, [Source.Owned] ref Joint.Base joint)
		{
			if (joint.flags.HasAll(Joint.Flags.Organic))
			{
				//joint.torque_modifier = organic.consciousness;

				//joint.max_stress_force_modifier = (health.integrity * health.integrity * health.integrity); // * (health.durability * health.durability);
				//joint.max_stress = (health.integrity * health.integrity * health.integrity * health.integrity);

				//if (organic_state.rotten > 0.80f)
				//{
				//	joint.max_stress_force_modifier *= 0.00f;
				//	joint.max_stress *= 0.00f;
				//}

				//var rotten_modifier = 1.00f - organic_state.rotten;

				//joint.torque_modifier = Maths.Clamp(modifier, 0.00f, 1.00f);

				//rotary_limit.min = Maths.Lerp(-MathF.PI * 0.20f, 0.00f, modifier);
				//rotary_limit.max = Maths.Lerp(+MathF.PI * 0.20f, 0.00f, modifier);

				//if (rotten_modifier)

				//joint.max_stress_force_modifier *= rotten_modifier * rotten_modifier * rotten_modifier;
				//joint.max_stress *= rotten_modifier * rotten_modifier * rotten_modifier;

				if (!joint.state.HasAll(Joint.State.Attached))
				{
					organic_state.pain += 30.00f;

#if CLIENT
					//if (organic_state.consciousness_shared_new > 0.30f)

					if (Camera.IsVisible(Camera.CullType.Rect1x, transform.position))
					{
						ref var region = ref info.GetRegion();
						var random = XorRandom.New();

						//App.WriteLine("bleed");

						if (random.NextBool(Maths.Lerp01(0.30f, 0.00f, organic_state.rotten * 15.00f)))
						{
							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = Organic.tex_blood,
								lifetime = random.NextFloatRange(0.50f, 0.80f),
								pos = transform.LocalToWorld(joint.offset_a) + random.NextVector2(0.20f),
								vel = new Vector2(0, 1) + random.NextUnitVector2Range(0.20f, 4.50f), // (transform.GetDirection() + random.NextVector2(0.50f)) * random.NextFloatRange(0.00f, 1.00f),
								fps = random.NextByteRange(4, 8),
								frame_count = 16,
								frame_count_total = 16,
								frame_offset = random.NextByteRange(0, 4),
								scale = random.NextFloatRange(1.00f, 1.50f),
								//rotation = random.NextFloat(10.00f),
								//angular_velocity = random.NextFloat(0.50f),
								growth = random.NextFloatRange(-0.50f, 0.20f),
								drag = random.NextFloatRange(0.10f, 0.20f),
								color_a = new Color32BGRA(0xcc900000),
								color_b = new Color32BGRA(0x00900000),
								force = new Vector2(0.00f, 20.00f),
								stretch = new Vector2(random.NextFloatRange(0.20f, 0.50f), random.NextFloatRange(0.80f, 1.00f)),
								face_dir_ratio = 1.00f
							});
						}
					}
#endif
				}

				//var health_norm = health.integrity * health.durability;
				//joint.max_stress_modifier = (health_norm + 0.20f).Clamp01().Pow2().Pow2();
			}
		}
	}
}