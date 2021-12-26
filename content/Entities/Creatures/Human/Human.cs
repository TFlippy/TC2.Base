namespace TC2.Base.Components
{
	public static class Human
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public struct Data: IComponent
		{
			[Save.Ignore, Net.Ignore] public float next_groan;
		}

		public static readonly Sound.Handle[] groan_sounds = 
		{
			"death_long_0",
		};

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateGroan(ISystem.Info info, [Source.Owned] ref Human.Data human, [Source.Owned] ref Organic.Data organic, [Source.Owned] ref Organic.State organic_state, [Source.Owned] ref Head.Data head, [Source.Owned] in Transform.Data transform)
		{
			if (info.WorldTime > human.next_groan)
			{
				//App.WriteLine(organic_state.pain_shared);
				if (organic_state.pain_shared > 500.00f && organic_state.consciousness_shared > 0.30f)
				{
#if SERVER
					ref var region = ref info.GetRegion();
					var random = XorRandom.New();
					Sound.Play(ref region, Human.groan_sounds.GetRandom(ref random), transform.position, volume: 0.20f + Maths.Clamp(organic_state.pain_shared / 2000.00f, 0.20f, 0.50f), pitch: random.NextFloatRange(0.70f, 1.10f) * (1.00f + Maths.Clamp((organic_state.pain_shared_new / 2000.00f), 0.00f, 0.10f)));
					human.next_groan = info.WorldTime + (Maths.Clamp(10.00f - (organic_state.pain_shared / 300.00f), 5.00f, 10.00f) * random.NextFloatRange(0.80f, 1.30f));
#endif
				}
			}
		}
	}
}
