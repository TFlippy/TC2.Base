
namespace TC2.Base.Components
{
	public static partial class Brain
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,
			}


		}
	}

	public static partial class OrganicExt
	{
		[Shitcode]
		[ISystem.PostUpdate.A(ISystem.Mode.Single, ISystem.Scope.Region, order: 5, flags: ISystem.Flags.Unchecked)]
		public static void UpdateBrain([Source.Owned, Original] ref Organic.Data organic_original, [Source.Owned, Override] in Organic.Data organic_override,
		[Source.Owned] ref Organic.State organic_state,
		[Source.Owned] in Health.Data health, [Source.Owned] bool dead)
		{
			const float pain_cutoff = 150.00f;

			if (organic_original.tags.HasAny(Organic.Tags.Brain))
			{
				var p = (dead || organic_state.pain_shared < pain_cutoff) ? 0.00f : Maths.PowFast(Maths.Max(0.00f, organic_state.pain_shared - pain_cutoff) * 0.0018f, 1.50f) * 0.10f;
				//organic_original.consciousness = Maths.Lerp(organic_original.consciousness, 1.00f - Maths.Clamp01(p), 0.02f); // player.flags.HasAll(Player.Flags.Alive) ? 1.00f : 0.30f;
				organic_original.consciousness = Maths.Lerp2(organic_original.consciousness, Maths.Min(1.00f - (Maths.Clamp01(p) * 0.85f), 1.00f - (organic_state.stun_norm * 0.60f)), 0.10f, 0.02f); // player.flags.HasAll(Player.Flags.Alive) ? 1.00f : 0.30f;

				var health_norm = health.GetHealthNormalized();
				if (dead || health_norm < 0.40f)
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
					organic_state.motorics_shared_new = Maths.Max(organic_state.consciousness_shared_new - Maths.Clamp(p, 0.00f, 0.35f), 0.00f) * (1.00f - organic_state.stun_norm);
				}
			}
		}

		[ISystem.PreUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region, order: 10, flags: ISystem.Flags.Unchecked), HasComponent<Head.Data>(Source.Modifier.Owned, false)]
		public static void UpdateConnected(Entity ent_organic_parent, Entity ent_organic_child,
		[Source.Parent, Override] in Organic.Data organic_parent, [Source.Parent] ref Organic.State organic_state_parent,
		[Source.Owned, Override] in Organic.Data organic_child, [Source.Owned] ref Organic.State organic_state_child,
		[Source.Parent] in Joint.Base joint)
		{
			if (joint.flags.HasAny(Joint.Flags.Organic))
			{
				organic_state_child.consciousness_shared_new = Maths.Max(organic_state_child.consciousness_shared_new, organic_state_parent.consciousness_shared);
				organic_state_child.motorics_shared_new = Maths.Max(organic_state_child.motorics_shared_new, organic_state_parent.motorics_shared);
				organic_state_child.unconscious_time = organic_state_parent.unconscious_time;

				organic_state_parent.pain_shared_new = organic_state_child.pain_shared_new = Maths.Max(organic_state_parent.pain_shared_new, organic_state_child.pain_shared_new);
			}
		}

		// TODO: Shitcoded workaround so head always updates after other body parts (otherwise it won't affect consciousness, in case the system runs on the head first)
		[ISystem.PreUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region, order: 15, flags: ISystem.Flags.Unchecked), HasComponent<Head.Data>(Source.Modifier.Owned, true)]
		public static void UpdateConnectedHead(Entity ent_organic_parent, Entity ent_organic_child,
		[Source.Parent, Override] in Organic.Data organic_parent, [Source.Parent] ref Organic.State organic_state_parent,
		[Source.Owned, Override] in Organic.Data organic_child, [Source.Owned] ref Organic.State organic_state_child,
		[Source.Parent] in Joint.Base joint)
		{
			if (joint.flags.HasAny(Joint.Flags.Organic))
			{
				organic_state_parent.consciousness_shared_new = organic_state_child.consciousness_shared;
				organic_state_parent.motorics_shared_new = organic_state_child.motorics_shared;
				organic_state_parent.unconscious_time = organic_state_child.unconscious_time;

				organic_state_parent.pain_shared_new = organic_state_child.pain_shared_new = Maths.Max(organic_state_parent.pain_shared_new, organic_state_child.pain_shared_new);
			}
		}


		[ISystem.PostUpdate.B(ISystem.Mode.Single, ISystem.Scope.Region, flags: ISystem.Flags.Unchecked)]
		public static void Update2(ISystem.Info info, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state, 
		[Source.Owned] in Health.Data health, [Source.Owned] bool dead)
		{
			organic_state.consciousness_shared = Maths.Lerp(organic_state.consciousness_shared, organic_state.consciousness_shared_new, 0.20f);
			organic_state.motorics_shared = Maths.Lerp(organic_state.motorics_shared, organic_state.motorics_shared_new, 0.20f);
			organic_state.pain_shared = Maths.Lerp(organic_state.pain_shared, organic_state.pain_shared_new * organic.pain_modifier, 0.20f);
			organic_state.pain = Maths.Lerp(organic_state.pain, organic_state.pain * (0.15f + (Maths.Max(0.00f, 0.60f - Maths.Pow6(health.GetHealthNormalized()) * 0.90f))).Clamp01() * organic.pain_modifier, 0.008f);
			organic_state.stun = Maths.MoveTowards(organic_state.stun, 0.00f, info.DeltaTime * 50.00f);

			organic_state.stun_norm = Maths.Normalize01(organic_state.stun, 500.00f).Pow2();
			organic_state.efficiency = organic_state.motorics_shared.Clamp01() * health.GetHealthNormalized();
		}

		[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region, flags: ISystem.Flags.Unchecked)]
		public static void UpdateConsciousness(ISystem.Info info, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			if (organic_state.consciousness_shared > 0.20f)
			{
				organic_state.unconscious_time = 0.00f;
			}
			else
			{
				organic_state.unconscious_time += App.fixed_update_interval_s * ((1.00f - Maths.Normalize01Fast(organic_state.consciousness_shared, 0.20f)) * 4.50f);
			}
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region, flags: ISystem.Flags.Unchecked)]
		public static void UpdateJoint([Source.Shared] in Organic.State organic_state, [Source.Owned] ref Joint.Base joint)
		{
			if (joint.flags.HasAny(Joint.Flags.Organic))
			{
				var modifier = Maths.Lerp01(organic_state.efficiency * organic_state.efficiency, organic_state.consciousness_shared * 1.50f, 0.50f) * (1.00f - organic_state.stun_norm);
				joint.torque_modifier = modifier;
			}
		}

		[HasTag("dead", true, Source.Modifier.Owned), HasComponent<Organic.Data>(Source.Modifier.Owned, true)]
		[ISystem.PreUpdate.B(ISystem.Mode.Single, ISystem.Scope.Region, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit)]
		public static void UpdateNoRotate([Source.Owned, Override] ref NoRotate.Data no_rotate)
		{
			no_rotate.multiplier = 0.00f;
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region, flags: ISystem.Flags.Unchecked | ISystem.Flags.SkipLocalsInit)]
		public static void UpdateArm<T>([Source.Shared, Override] in Organic.Data organic, [Source.Shared] ref Organic.State organic_state,
		[Source.Owned] in Arm.Data arm, [Source.Owned, Override] ref T joint) where T : unmanaged, IComponent, IRotaryJoint
		{
			joint.MaxSpeed *= Maths.Clamp(organic.motorics, 0.30f, 1.50f);
			joint.MaxBias += (1.00f - organic.motorics.Clamp01()) * 0.15f;
		}

		// TODO: this is bad
		[ISystem.Update.F(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateArmAimable(
		[Source.Parent, Override] in Organic.Data organic, [Source.Parent] ref Arm.Data arm, [Source.Parent, Override] ref Joint.Gear joint_gear,
		[Source.Owned] in Aimable.Data aimable, [Source.Parent] in Body.Data body_parent, [Source.Owned] in Body.Data body_child)
		{
			var aim_torque = arm.aim_torque;
			if (arm.aim_torque > 1.00f)
			{
				aim_torque += (body_parent.GetMass() / (arm.aim_torque * 0.000018f)) * organic.strength;
			}

			//App.WriteLine(aim_torque);

			joint_gear.min = Maths.Max(joint_gear.min, aimable.min);
			joint_gear.max = Maths.Min(joint_gear.max, aimable.max);
			joint_gear.step = Maths.Clamp(body_child.GetAngularMassInv() * aim_torque * App.fixed_update_interval_s, 0.50f, joint_gear.step);
		}

		[ISystem.Update.E(ISystem.Mode.Single, ISystem.Scope.Region, flags: ISystem.Flags.Unchecked)]
		public static void UpdateFacing([Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state, [Source.Owned] ref Facing.Data facing)
		{
			facing.flags.SetFlag(Facing.Flags.Disabled, organic_state.consciousness_shared < 0.50f || organic_state.stun_norm >= 0.50f);
		}

		//[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		//[HasTag("dead", true, Source.Modifier.Parent), HasComponent<Organic.Data>(Source.Modifier.Parent, true)]
		//public static void OnUpdateArmDead([Source.Owned] ref Holdable.Data holdable, [Source.Parent] ref Arm.Data arm, 
		//[Source.Parent] ref Joint.Base joint_base)
		//{
		//	//joint_base.force_modifier_attached = 0.00f;
		//	joint_base.stress_modifier_attached *= 10.00f;
		//	joint_base.stress_accumulator = 4.00f;
		//	//joint_base.torque_modifier_attached = 0.00f;
		//}

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region)]
		[HasTag("dead", true, Source.Modifier.Shared), HasComponent<Organic.Data>(Source.Modifier.Shared, true), HasComponent<Arm.Data>(Source.Modifier.Owned, true)]
		public static void OnAddArmDead([Source.Owned] ref Joint.Base joint_base)
		{
			joint_base.state.AddFlag(Joint.State.Force_Detach);
		}

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", true, Source.Modifier.Owned)]
		public static void OnAddBodyDead(Entity entity, [Source.Owned, Original] in Organic.Data organic, [Source.Owned] ref Body.Data body)
		{
			//App.WriteLine("add body layer dead");

			body.override_shape_layer.AddFlag(Physics.Layer.Dead);
			body.MarkDirty();

			//App.WriteLine($"rip {torque}");
			//body.AddTorque(torque);

			var random = XorRandom.New(unchecked(entity.id.Ror(17) * XorRandom.magix_mul));
			var torque = body.GetAngularMass() * random.NextFloatRange(20.00f, 25.00f).WithSign(random.NextBool(0.50f)) * App.tickrate_f32;
			body.AddTorque(torque);
			body.AddForceWorld(random.NextUnitVector2Range(0.50f, 0.90f) * body.GetMass() * App.tickrate_f32 * 10.00f, body.GetPosition() + random.NextUnitVector2Range(0.15f, 0.20f));
		}

		[ISystem.Remove(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", true, Source.Modifier.Owned)]
		public static void OnRemove_BodyDead([Source.Owned, Original] in Organic.Data organic, [Source.Owned] ref Body.Data body)
		{
			//App.WriteLine("remove body layer dead");

			body.override_shape_layer.RemoveFlag(Physics.Layer.Dead);
			body.MarkDirty();
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.50f, flags: ISystem.Flags.Unchecked), HasTag("dead", true, Source.Modifier.Owned)]
		public static void OnUpdate_Rotting(ISystem.Info info, Entity entity,
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			organic_state.rotten = Maths.Max(organic_state.rotten + (info.DeltaTime * Constants.Organic.rotting_speed * 0.005f), 0.00f);

#if SERVER
			//App.WriteLine($"{organic_state.rotten} {entity}");

			if (organic_state.rotten >= 1.00f)
			{
				entity.Delete();
			}
#endif
		}

#if SERVER
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.20f, flags: ISystem.Flags.Unchecked)]
		public static void OnUpdate_Dead(Entity entity, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state, [Source.Owned] bool dead)
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

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region, order: 25), HasTag("dead", true, Source.Modifier.Parent)]
		public static void OnAdd_ParentDead(Entity ent_organic_state_child, Entity ent_organic_state_parent,
		[Source.Owned] ref Organic.State organic_state_child,
		[Source.Parent] ref Organic.State organic_state_parent,
		[Source.Parent] in Joint.Base joint)
		{
			if (joint.flags.HasAny(Joint.Flags.Organic))
			{
				ent_organic_state_child.AddTag("dead");
			}
		}
#endif

		//[HasComponent<Health.Data>(Source.Modifier.Shared, true)]
		//[ISystem.PreUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region, flags: ISystem.Flags.Unchecked)]
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region, flags: ISystem.Flags.Unchecked)]
		public static void OnUpdate_JointPain(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		[Source.Shared] in Transform.Data transform, [Source.Shared] in Health.Data health, [Source.Shared] ref Organic.State organic_state,
		[Source.Owned] ref Joint.Base joint)
		{
			if (joint.flags.HasAny(Joint.Flags.Organic))
			{
				if (joint.state.HasNone(Joint.State.Attached | Joint.State.Attaching))
				{
					organic_state.pain += 30.00f;

#if CLIENT
					//if (organic_state.consciousness_shared_new > 0.30f)
					var rotten_15x = organic_state.rotten * 15.00f;
					if (rotten_15x < 1.00f)
					{
						if (Camera.IsVisible(Camera.CullType.Rect1x, transform.position))
						{
							//App.WriteLine("bleed");

							if (random.NextBool(Maths.Lerp(0.30f, 0.00f, rotten_15x)))
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
					}
#endif
				}
			}
		}

		[ISystem.PostUpdate.A(ISystem.Mode.Single, ISystem.Scope.Region, flags: ISystem.Flags.Unchecked)]
		public static void OnUpdate_JointHealth([Source.Owned] ref Organic.State organic_state, [Source.Owned] in Health.Data health_child, 
		[Source.Parent] in Health.Data health_parent, [Source.Parent] ref Joint.Base joint)
		{
			if (joint.flags.HasAny(Joint.Flags.Organic))
			{

				const float health_threshold = 0.80f;

				//var mult = (1.00f - health.GetHealthNormalized()).Pow2();

				//var health_norm = Maths.Min(health_child.GetHealthNormalizedAvg(), health_parent.GetHealthNormalizedAvg());
				var health_norm = Maths.Max(Maths.LerpFMA(health_child.integrity, health_child.durability, 0.25f), Maths.LerpFMA(health_parent.integrity, health_parent.durability, 0.25f));
				//var mult = (1.00f - health_norm.Pow2()).Pow2();
				var mult = health_norm; //.Pow2();
				//joint.stress += joint.max_stress * mult;
				//joint.stress += joint.stress_threshold * mult * 0.80f; // * 1.05f;
				joint.stress_threshold_modifier *= mult;
				//joint.stress

				//var health_norm = health.GetHealthNormalized();
				//if (health_norm < health_threshold)
				//{
				//	var mult = health_norm.NormalizeFastUnsafe(health_threshold).Pow2();
				//	joint.force_modifier_attached *= mult;
				//}
				//joint.stress *= 1.00f + stress_mult;
				//joint.stress_accumulator += joint.displacement_smoothed * (1.50f + stress_mult);

			}
		}
	}
}