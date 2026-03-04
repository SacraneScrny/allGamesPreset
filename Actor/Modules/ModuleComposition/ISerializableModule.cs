namespace Sackrany.Actor.Modules.ModuleComposition
{
    public interface ISerializableModule
    {
        public object[] Serialize();
        public void Deserialize(object[] data);
    }
}