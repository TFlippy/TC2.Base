
namespace TC2.Base.Components
{
	public static partial class Climber_DEV
	{
		public static Sound.Handle[] snd_walljump =
		{
			"walljump.00",
			"walljump.01",
			"walljump.02"
		};

		// Crappily exposed Climber.cs for now, since it interacts with physics constraint pointers
		[ISystem.Update.E(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state,
		[Source.Owned] ref Climber.Data climber, [Source.Owned] in Health.Data health, [Source.Owned] ref Body.Data body, 
		[Source.Owned] ref Control.Data control, [Source.Parent, Optional] in Joint.Base joint_base,
		[Source.Owned] in Physics.Data physics, [Source.Owned, Pair.Component<Physics.Data>, Optional] in Net.Synchronized synchronized)
		{
			var dt = info.DeltaTime;

			//var ts = Timestamp.Now();

			//App.WriteLine(entity.GetFullName());

			//var has_authority = synchronized.HasAuthority();
			ref var keyboard = ref control.keyboard;
			var vel_delta = (body.GetVelocity() * dt);

			climber.MaxForce = 0.00f;
			climber.OffsetA = body.GetPosition() + vel_delta;
			climber.OffsetB = body.GetCenterOfGravity();
			climber.cling_entity = default;

			//if (!has_authority)
			//{

			//	return;
			//}

			//App.WriteLine(climber.wallclimb_timer);

//#if CLIENT
//			region.DrawDebugCircle(climber.OffsetA, 0.125f, Color32BGRA.Yellow, filled: true); // arbiter.GetContact(0), 1.00f, Color32BGRA.Green);
//			region.DrawDebugDir(body.GetPosition(), body.GetVelocity(), Color32BGRA.Yellow); // arbiter.GetContact(0), 1.00f, Color32BGRA.Green);
//#endif

			var is_climbing = false;
			var is_wallclimbing = false;
			var can_move = !keyboard.GetKey(Keyboard.Key.NoMove | Keyboard.Key.X) && (joint_base.flags.IsEmpty() || joint_base.flags.HasAny(Joint.Flags.Organic)); // && !body.GetParent().IsValid();
			var can_wallclimb = can_move && keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight);

			var force = Vector2.Zero; // new Vector2(0, float.Epsilon);
			var normal = Vector2.Zero;
			var climb_vel = Vector2.Zero;
			var climb_force = 0.00f;

			if (can_move && body.HasArbiters())
			{
				foreach (var arbiter in body.GetArbiters())
				{

//#if CLIENT
//					//region.DrawLine(arbiter.GetContact(0), arbiter.GetContact(0) + arbiter.GetNormal(), Color32BGRA.Green);
//					region.DrawDebugCircle(arbiter.GetContactPosition(0), 0.125f, Color32BGRA.Green, filled: true);
//#endif

					var layer = arbiter.GetLayer();
					if (layer.HasNone(Physics.Layer.Bounds))
					{
						if (!is_climbing && layer.HasAny(Physics.Layer.Climbable))
						{
							if (arbiter.GetShapeTags().Evaluate(climber.climbable_tags_mask))
							{
								var ent_arbiter = arbiter.GetEntity();

								var cling_force = climber.cling_force;
								var climb_speed = climber.climb_speed;
								cling_force *= Maths.Cutoff(organic_state.efficiency * organic_state.consciousness_shared, 0.30f, 0.00f) * organic.strength * (1.00f - organic_state.stun_norm);
								climber.cling_entity = ent_arbiter;
								climber.pos_climbable = arbiter.GetBodyPosition();

								if (keyboard.GetKey(Keyboard.Key.MoveLeft)) climb_vel.X -= 1.50f;
								if (keyboard.GetKey(Keyboard.Key.MoveRight)) climb_vel.X += 1.50f;
								if (keyboard.GetKey(Keyboard.Key.MoveUp)) climb_vel.Y -= 1.00f;
								if (keyboard.GetKey(Keyboard.Key.MoveDown)) climb_vel.Y += 1.50f;

								climb_vel *= climb_speed;

								climber.MaxForce = cling_force;
								climber.OffsetA += (climb_vel * dt);

								if (climb_vel != Vector2.Zero) body.Activate();

								is_climbing = true;
							}
							else
							{
								arbiter.SetIgnored();
							}
							//break;
						}
						//else if (!is_wallclimbing && can_wallclimb && arbiter.GetRigidityDynamic() > 0.90f)
						else if (can_wallclimb && arbiter.GetRigidityDynamic() > 0.90f)
						{
							normal += arbiter.GetNormal();
							normal = normal.GetNormalizedFast();

							var dot = Maths.Abs(normal.X);
							if (dot >= 0.90f)
							{
								//var f = dot * arbiter.GetFriction() * climber.climb_force;
								climb_force = Maths.Max(climb_force, dot * arbiter.GetFriction() * climber.climb_force);
								is_wallclimbing = true;
							}

//#if CLIENT
//							region.DrawDebugText(arbiter.GetContactPosition(0), $"{dot:0.00}", Color32BGRA.Green);
//							region.DrawDebugLine(arbiter.GetContactPosition(0), arbiter.GetContactPosition(0) + arbiter.GetNormal(), Color32BGRA.Green);
//#endif
						}
						else if (!is_climbing && layer.HasAny(Physics.Layer.Water))
						{
							var ent_arbiter = arbiter.GetEntity();

							var swim_force_modifier = climber.climb_force * 0.40f;

							var vel = Vector2.Zero;

							if (keyboard.GetKey(Keyboard.Key.MoveLeft)) vel.X -= 1.20f;
							if (keyboard.GetKey(Keyboard.Key.MoveRight)) vel.X += 1.20f;
							if (keyboard.GetKey(Keyboard.Key.MoveUp)) vel.Y -= 0.50f;
							if (keyboard.GetKey(Keyboard.Key.MoveDown)) vel.Y += 8.00f;

							vel *= 0.10f;

							if (vel.LengthSquared() > 0.01f) body.Activate();

							var swim_force = vel * swim_force_modifier;
							swim_force = Physics.LimitForce2B(ref body, swim_force, new Vector2(5, 15));

							body.AddForce(swim_force);
						}
					}
				}
			}

			var climb_vel_norm = climb_vel.GetNormalizedFast();

//#if CLIENT
//			region.DrawDebugDir(body.GetPosition(), normal.GetPerpendicularAligned(climb_vel_norm), Color32BGRA.Red);
//#endif

			if (is_climbing && normal != default) climber.OffsetA += normal.GetPerpendicularAligned(climb_vel_norm);

			if (can_wallclimb & is_wallclimbing)
			{
				if (climber.wallclimb_timer <= 0.50f)
				{
					climb_force += climber.climb_force;
					climb_force *= organic_state.efficiency * organic.strength * (1.00f - organic_state.stun_norm);

					var max_speed = climber.max_speed;

					normal = normal.GetNormalizedFast(out var normal_len_sq);
					if (normal_len_sq > 0.01f && info.WorldTime >= climber.last_walljump + 0.20f)
					{
						var do_walljump = keyboard.GetKeyDown(float.IsPositive(normal.X) ? Keyboard.Key.MoveLeft : Keyboard.Key.MoveRight);
						if (do_walljump)
						{
							//App.WriteLine(do_walljump);

							force.X *= normal.X * 50.00f;
							climb_force *= 50.00f;
							max_speed.X *= 2.00f;
							max_speed.Y *= 2.50f;

							climber.last_walljump = info.WorldTime;

#if CLIENT
							Sound.Play(snd_walljump.GetRandom(ref random), transform.position, volume: random.NextFloatRange(0.22f, 0.25f), pitch: random.NextFloatRange(0.85f, 0.95f));
#endif
						}
					}

					force.Y += -climb_force;
					force = Physics.LimitForce2B(ref body, force, max_speed);

					//App.WriteLine("climb");

					body.AddForce(force);

					climber.last_wallclimb = info.WorldTime;
					climber.last_force = force;

					//climber.wallclimb_timer += info.DeltaTime;

					//if (climber.wallclimb_timer >= 0.50f)
					//{
					//	climber.wallclimb_timer *= -1.00f;
					//}


					climber.wallclimb_timer += info.DeltaTime;
				}
			}
			else
			{
				climber.wallclimb_timer = Maths.MoveTowards(climber.wallclimb_timer, 0.00f, info.DeltaTime * 0.70f);
			}

//#if CLIENT
//			region.DrawText(body.GetPosition(), $"{climber.wallclimb_timer:0.00}", Color32BGRA.White);
//#endif
		}
	}
}
