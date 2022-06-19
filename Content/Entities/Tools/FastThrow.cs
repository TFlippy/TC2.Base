
namespace TC2.Base.Components
{
	public static partial class FastThrow
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			[Statistics.Info("Throwing Speed", description: "Additional speed added to this object when thrown", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float added_speed = 10.00f;

			[Save.Ignore, Net.Ignore] public bool last_attach = false;

			public Data()
			{

			}
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity, [Source.Owned] ref FastThrow.Data throwing, [Source.Owned] ref Body.Data body, 
		[Source.Owned] in Transform.Data transform)
		{

#if SERVER

			var ent_holder = entity.GetParent(Relation.Type.Child);

			if (ent_holder.IsAlive())
			{
				if(throwing.last_attach != true)
				{
					//App.WriteLine("attach");
				 	throwing.last_attach = true;
				}
			}
			else if(throwing.last_attach)
			{
				//App.WriteLine("speed");
				var dir = body.GetVelocity();
				dir = dir.GetNormalized();
				body.AddForce(dir * Maths.Min(body.GetMass(), 20.0f) * throwing.added_speed * App.tickrate);
				throwing.last_attach = false;
				body.Sync<Body.Data>(entity);
			}
#endif
		}
	}
}
