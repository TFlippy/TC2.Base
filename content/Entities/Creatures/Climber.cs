
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
		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned, Pair.Of<Climber.Data>] ref Shape.Circle shape,
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state,
		[Source.Owned] ref Climber.Data climber, [Source.Owned] in Health.Data health, [Source.Owned] ref Body.Data body, [Source.Owned] ref Control.Data control)
		{
			//var ts = Timestamp.Now();

			ref var keyboard = ref control.keyboard;

			climber.MaxForce = 0.00f;
			climber.OffsetA = body.GetPosition() + (body.GetVelocity() * App.fixed_update_interval_s);
			climber.cling_entity = default;

			//App.WriteLine(climber.wallclimb_timer);

			var is_climbing = false;
			var is_wallclimbing = false;
			var can_move = !keyboard.GetKey(Keyboard.Key.NoMove) && !body.GetParent().IsValid();
			var can_wallclimb = can_move && keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight);

			var force = new Vector2(0, float.Epsilon);
			var normal = new Vector2(0, 0);
			var climb_force = 0.00f;

			if (can_move && body.HasArbiters())
			{
				foreach (var arbiter in body.GetArbiters())
				{

//#if CLIENT
//					region.DrawLine(arbiter.GetContact(0), arbiter.GetContact(0) + arbiter.GetNormal(), Color32BGRA.Green);
//					region.DrawCircle(arbiter.GetContact(0), 1.00f, Color32BGRA.Green);
//#endif

					var layer = arbiter.GetLayer();
					if (!layer.HasAny(Physics.Layer.Bounds))
					{
						if (!is_climbing && layer.HasAny(Physics.Layer.Climbable) && !layer.HasAny(Physics.Layer.Tree))
						{
							var ent_arbiter = arbiter.GetEntity();

							var cling_force = climber.cling_force;
							var climb_speed = climber.climb_speed;
							cling_force *= Maths.Cutoff(organic_state.efficiency * organic_state.consciousness_shared, 0.30f, 0.00f) * organic.strength;
							climber.cling_entity = ent_arbiter;
							climber.pos_climbable = arbiter.GetBodyPosition();

							var vel = Vector2.Zero;

							if (keyboard.GetKey(Keyboard.Key.MoveLeft)) vel.X -= 1.50f;
							if (keyboard.GetKey(Keyboard.Key.MoveRight)) vel.X += 1.50f;
							if (keyboard.GetKey(Keyboard.Key.MoveUp)) vel.Y -= 1.00f;
							if (keyboard.GetKey(Keyboard.Key.MoveDown)) vel.Y += 1.50f;

							vel *= climb_speed;

							climber.MaxForce = cling_force;
							climber.OffsetA += (vel * App.fixed_update_interval_s);

							if (vel != Vector2.Zero) body.Activate();

							is_climbing = true;
							//break;
						}
						else if (can_wallclimb && arbiter.GetRigidityDynamic() > 0.90f)
						{
							normal += arbiter.GetNormal();

							var dot = MathF.Abs(normal.X);
							if (dot >= 0.90f)
							{
								var f = MathF.Abs(normal.X) * arbiter.GetFriction() * climber.climb_force;
								climb_force = MathF.Max(climb_force, f);
								is_wallclimbing = true;
							}

//#if CLIENT
//							region.DrawText(arbiter.GetContact(0), $"{dot:0.00}", Color32BGRA.Green);
//							region.DrawLine(arbiter.GetContact(0), arbiter.GetContact(0) + arbiter.GetNormal(), Color32BGRA.Green);
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
							swim_force = Physics.LimitForce(ref body, swim_force, new Vector2(5, 15));

							body.AddForce(swim_force);
						}
					}
				}
			}

			if (can_wallclimb && is_wallclimbing)
			{
				if (climber.wallclimb_timer <= 0.50f)
				{
					climb_force *= climber.climb_force;
					climb_force *= organic_state.efficiency * organic.strength;

					var max_speed = new Vector2(10, 10);

					normal = normal.GetNormalized(out var normal_len);
					if (normal_len > 0.01f && info.WorldTime >= climber.last_walljump + 0.20f)
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
					force = Physics.LimitForce(ref body, force, max_speed);

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
