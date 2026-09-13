var p = UnityEngine.Object.FindObjectOfType<Level04BParkourController>();
var dog = PlayerActors.Instance.Dog;
var runner=p.GetComponent<Level04BParkourDogRunner>();
var path=(UnityEngine.Transform[])runner.GetType().GetField("chasePath",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(runner);
var samples=path.Select(t=> new {name=t.name,position=t.position.ToString("F3"),hits=UnityEngine.Physics.RaycastAll(t.position+UnityEngine.Vector3.up*3,UnityEngine.Vector3.down,6,UnityEngine.Physics.DefaultRaycastLayers,UnityEngine.QueryTriggerInteraction.Ignore).Select(h=>h.collider.name+" y="+h.point.y).ToArray()}).ToArray();
return Codely.Newtonsoft.Json.JsonConvert.SerializeObject(new {offset=dog.MoverAttachOffset.ToString(),colliders=dog.GetComponentsInChildren<UnityEngine.Collider>().Select(c=>c.name+" "+c.bounds.ToString()).ToArray(),samples});
