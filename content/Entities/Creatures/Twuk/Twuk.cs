namespace TC2.Base.Components
{
	public static partial class Twuk
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			[Save.Ignore, Net.Ignore] public float t_next_sound;
		}

		public static Sound.Handle[] sounds_idle =
		{
			"twuk.idle.00",
			"twuk.idle.01",
			"twuk.idle.02"
		};

		public static Sound.Handle[] sounds_screech =
		{
			"twuk.screech.00",
			"twuk.screech.01",
			"twuk.screech.02",
			"twuk.screech.03"
			//"twuk.screech.04"
		};

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.2333f), HasTag("twuk", true, Source.Modifier.Parent), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateTwuk(ISystem.Info info, ref XorRandom random, ref Region.Data region, 
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Head.Data head, 
		[Source.Parent] ref Twuk.Data twuk, [Source.Parent] ref NPC.Data npc)
		{
			if (info.WorldTime >= twuk.t_next_sound)
			{
				if (npc.self_hints.HasAny(NPC.SelfHints.Is_Alerted))
				{
					Sound.Play(ref region, sounds_screech.GetRandom(ref random), transform.position, volume: random.NextFloatRange(0.60f, 0.90f), pitch: random.NextFloatRange(0.80f, 1.20f) * head.voice_pitch, size: 0.50f, dist_multiplier: random.NextFloatRange(0.90f, 1.10f));
					twuk.t_next_sound = info.WorldTime + random.NextFloatRange(1.04f, 1.92f);
				}
				else
				{
					Sound.Play(ref region, sounds_idle.GetRandom(ref random), transform.position, volume: random.NextFloatRange(0.40f, 0.50f), pitch: random.NextFloatRange(0.80f, 1.40f) * head.voice_pitch, size: 0.50f, dist_multiplier: random.NextFloatRange(0.50f, 0.60f));
					twuk.t_next_sound = info.WorldTime + random.NextFloatRange(4.00f, 12.00f);
				}
			}
		}
#endif
	}
}
