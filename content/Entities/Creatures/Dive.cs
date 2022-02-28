
namespace TC2.Base.Components
{
	public static partial class Dive
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Dive.State>()]
		public partial struct Data: IComponent
		{
			public Sound.Handle sound;

			public float cooldown = 1.00f;
			public float speed = 20.00f;
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			[Net.Ignore, Save.Ignore] public float next_dive;
		}

		[ISystem.LateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateNoRotate(ISystem.Info info, [Source.Owned] in Organic.Data organic, [Source.Owned] ref NoRotate.Data no_rotate, [Source.Any] in Dive.State dive_state)
		{
			var elapsed = dive_state.next_dive - info.WorldTime;
			if (elapsed > 2.20f)
			{
				no_rotate.multiplier = 0.00f;
			}
			else
			{
				no_rotate.multiplier = 1.00f;
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void Update(ISystem.Info info,
		[Source.Owned] in Dive.Data dive, [Source.Owned] ref Dive.State dive_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform, [Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state)
		{
			if (organic_state.consciousness_shared > 0.60f && info.WorldTime > dive_state.next_dive && control.mouse.GetKey(Mouse.Key.Right))
			{
				ref var region = ref info.GetRegion();
				var random = XorRandom.New();

				dive_state.next_dive = info.WorldTime + dive.cooldown;

				var dir = ((control.mouse.position - transform.position) + new Vector2(0, -3)).GetNormalized();

				var force = dir * body.GetMass() * dive.speed * App.tickrate * organic_state.efficiency;
				force = Physics.LimitForce(ref body, force, new Vector2(dive.speed, dive.speed));

				body.AddForce(force);

#if SERVER
				Sound.Play(ref region, dive.sound, transform.position, volume: 0.80f, pitch: 1.00f, size: 2.00f, priority: 0.60f);
#endif
			}
		}
	}
}
