
namespace TC2.Base.Components
{
	public static partial class Dive
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Dive.State>()]
		public partial struct Data: IComponent
		{
			public Sound.Handle sound;

			public Vector2 offset;

			public float cooldown = 1.00f;
			public float speed = 20.00f;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			[Net.Ignore, Save.Ignore] public float next_dive;
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void Update(ISystem.Info info, ref XorRandom random,
		[Source.Owned] in Dive.Data dive, [Source.Owned] ref Dive.State dive_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state)
		{
			ref var region = ref info.GetRegion();

			var pos = transform.LocalToWorld(dive.offset);
			var dir = (control.mouse.position - pos).GetNormalized();

#if CLIENT
			region.DrawDebugDir(pos, dir, Color32BGRA.Red);
#endif

			if (organic_state.consciousness_shared > 0.60f && info.WorldTime > dive_state.next_dive && control.mouse.GetKey(Mouse.Key.Right))
			{

				dive_state.next_dive = info.WorldTime + dive.cooldown;

				//var dir = transform.GetDirection(); // (control.mouse.position - pos).GetNormalized();

				var force = dir * body.GetMass() * dive.speed * App.tickrate * organic_state.efficiency;
				force = Physics.LimitForce(ref body, force, new Vector2(dive.speed, dive.speed));

				body.AddForceWorld(force, pos + (dir * 0.50f));
				body.AddVelocity(new Vector2(0, -5 * MathF.Abs(dir.X)));

#if SERVER
				Sound.Play(ref region, dive.sound, pos, volume: 0.80f, pitch: 1.00f, size: 2.00f, priority: 0.60f);
#endif
			}
		}
	}
}
