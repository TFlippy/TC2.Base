
namespace TC2.Base.Components
{
	public static partial class Runner
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Grounded = 1 << 0,
			Walking = 1 << 1,
			Jumping = 1 << 2,
			Falling = 1 << 3,
			Crouching = 1 << 4,
			Climbing = 1 << 5,
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			public float walk_force;
			public float jump_force;
			public float max_speed = 10.00f;

			public float force_modifier = 1.00f;
			public float speed_modifier = 1.00f;

			[Save.Ignore] public float walk_modifier_current;
			[Save.Ignore] public float jump_force_current;
			[Save.Ignore] public Runner.Flags flags;

			[Save.Ignore, Net.Ignore] public float last_jump;
			[Save.Ignore, Net.Ignore] public float last_ground;
			[Save.Ignore, Net.Ignore] public float last_climb;
			[Save.Ignore, Net.Ignore] public float last_air;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateOrganic(ISystem.Info info, [Source.Owned] ref Runner.Data runner, [Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state)
		{
			runner.force_modifier = organic.strength;
			runner.speed_modifier = organic_state.efficiency;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateClimbing(ISystem.Info info, [Source.Owned] ref Runner.Data runner, [Source.Parent] in Climber.Data climber)
		{
			runner.flags.SetFlag(Runner.Flags.Climbing, climber.cling_entity.IsValid());
			if (runner.flags.HasAll(Runner.Flags.Climbing)) runner.last_climb = info.WorldTime;
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateMovement(ISystem.Info info, [Source.Owned] ref Runner.Data runner, [Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control)
		{
			ref readonly var keyboard = ref control.keyboard;

			var force = new Vector2(0, 0);
			var max_speed = new Vector2(runner.max_speed, 10);
			var velocity = body.GetVelocity();
			var mass = body.GetMass();

			var can_move = !keyboard.GetKey(Keyboard.Key.NoMove);
			var is_walking = can_move && keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight);

			runner.flags.SetFlag(Runner.Flags.Walking, is_walking);

			if (is_walking)
			{
				runner.walk_modifier_current = Maths.Lerp(runner.walk_modifier_current, 1.00f, 0.15f);

				if (velocity.X > -runner.max_speed && keyboard.GetKey(Keyboard.Key.MoveLeft)) force.X -= runner.walk_force * runner.walk_modifier_current;
				if (velocity.X < +runner.max_speed && keyboard.GetKey(Keyboard.Key.MoveRight)) force.X += runner.walk_force * runner.walk_modifier_current;
			}
			else
			{
				runner.walk_modifier_current = 0.00f;
			}

			var normal = default(Vector2);
			var layers = default(Physics.Layer);
			var friction = 0.00f;

			var arbiter_count = 0;
			foreach (var arbiter in body.GetArbiters())
			{
				normal += arbiter.GetNormal();
				friction += arbiter.GetFriction();
				layers |= arbiter.GetLayerB();

				arbiter_count++;
			}

			if (arbiter_count > 0)
			{
				normal /= arbiter_count;
				friction /= arbiter_count;
			}

			if (arbiter_count > 0 && normal.Y >= -0.40f && !layers.HasAll(Physics.Layer.Bounds))
			{
				runner.flags.SetFlag(Runner.Flags.Grounded, true);
				runner.last_ground = info.WorldTime;

				var dot = Vector2.Dot(normal, new Vector2(MathF.Sign(normal.X), 0));

				force.Y = -MathF.Abs(dot * force.X) * friction * 0.50f;
				force.X *= (1.00f - dot);
			}
			else
			{
				force.X *= 0.75f;
				force.Y *= 0.00f;
				runner.flags.SetFlag(Runner.Flags.Grounded, false);
				runner.last_air = info.WorldTime;
			}

			if (can_move && keyboard.GetKey(Keyboard.Key.MoveUp) && (info.WorldTime - runner.last_jump) > 0.40f && (((info.WorldTime - runner.last_ground) < 0.20f) || (((info.WorldTime - runner.last_climb) > 0.00f) && (info.WorldTime - runner.last_climb) < 0.20f)))
			{
				runner.jump_force_current = runner.jump_force;
				runner.last_jump = info.WorldTime;
			}

			force.Y -= runner.jump_force_current;
			runner.jump_force_current *= 0.50f;

			runner.flags.SetFlag(Runner.Flags.Crouching, can_move && control.keyboard.GetKey(Keyboard.Key.MoveDown) && (info.WorldTime - runner.last_ground) <= 0.25f);
			if (runner.flags.HasAll(Runner.Flags.Crouching))
			{
				runner.jump_force_current *= 0.50f;
				max_speed.X *= 0.50f;
			}

			if (!is_walking)
			{
				var required_force_dir = (velocity * mass * App.tickrate) - force;
				required_force_dir = required_force_dir.GetNormalized(out var required_force_magnitude);
				required_force_dir *= Maths.Clamp(runner.walk_force * 0.50f, -required_force_magnitude, required_force_magnitude);

				if (!runner.flags.HasAll(Runner.Flags.Grounded))
				{
					required_force_dir.X *= 0.02f;
					required_force_dir.Y *= 0.00f;
				}

				force -= required_force_dir;
			}

			max_speed *= runner.speed_modifier;
			force *= runner.force_modifier;

			force = Physics.LimitForce(ref body, force, max_speed);
			body.AddForce(force);
		}
	}
}
