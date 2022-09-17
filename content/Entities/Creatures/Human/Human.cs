namespace TC2.Base.Components
{
	public static partial class Human
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			[Save.Ignore, Net.Ignore] public float next_scream = default;

			public Data()
			{

			}
		}

		public static Sound.Handle[] snd_cough =
		{
			"kobold.cough.00",
			"kobold.cough.01",
			"kobold.cough.02",
			"kobold.cough.03",
			"kobold.grunt.00",
		};

		public static Sound.Handle[] snd_scream =
		{
			"kobold.scream.00",
			"kobold.scream.01",
		};

		[ISystem.EarlyUpdate(ISystem.Mode.Single), HasTag("human", true, Source.Modifier.Owned)]
		public static void UpdateHuman(ISystem.Info info, [Source.Owned] ref Human.Data human)
		{

		}

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		[HasTag("dead", false, Source.Modifier.Owned), HasTag("human", true, Source.Modifier.Parent)]
		public static void HeadUpdate(ISystem.Info info, Entity entity,
		[Source.Parent, Override] in Organic.Data organic, [Source.Parent] ref Organic.State organic_state, [Source.Parent] ref Human.Data human,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Head.Data head, [Source.Owned] ref Body.Data body)
		{
			ref var region = ref info.GetRegion();
			var time = info.WorldTime;
			var random = XorRandom.New();

			//App.WriteLine("hm");

			if (organic_state.unconscious_time > 0.50f)
			{
				head.next_sound = MathF.Min(head.next_sound, time + 2.50f);
			}

			var pain_delta = MathF.Max(organic_state.pain, 0.00f);
			//if (time >= human.next_scream && organic_state.consciousness_shared > 0.80f && pain_delta >= 1000.00f)
			//{
			//	if (random.NextBool(0.90f))
			//	{
			//		Sound.Play(ref region, snd_scream.GetRandom(ref random), transform.position, volume: 0.65f, pitch: random.NextFloatRange(0.70f, 1.00f) * head.voice_pitch);
			//		head.next_sound = time + random.NextFloatRange(1.50f, 3.00f);
			//	}
			//	human.next_scream = time + 30.00f;
			//}

			if (time >= head.next_sound)
			{
				if (organic_state.consciousness_shared > 0.10f && (organic_state.unconscious_time > 3.00f || (organic_state.efficiency < 0.50f && organic_state.pain > 50.00f)))
				{
					var lerp = Maths.NormalizeClamp(organic_state.unconscious_time, 10.00f);

					Sound.Play(ref region, snd_cough.GetRandom(ref random), transform.position, volume: 0.35f * Maths.Lerp(1.00f, 0.50f, lerp), pitch: random.NextFloatRange(0.80f, 1.00f) * Maths.Lerp(1.00f, 0.80f, lerp) * head.voice_pitch);
					head.next_sound = time + random.NextFloatRange(1.50f, 2.50f + lerp);

					body.AddImpulse(random.NextUnitVector2Range(50.00f, 150.00f));
				}
				else
				{
					if (organic_state.efficiency < 0.60f)
					{
						if (organic_state.pain > 2000.00f)
						{

						}
					}
					else
					{

					}
				}
			}
		}
#endif
	}
}
