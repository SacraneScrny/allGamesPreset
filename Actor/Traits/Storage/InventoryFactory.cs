namespace Sackrany.Actor.Traits.Storage
{
    public static class InventoryFactory
    {
        public static Inventory CreateInventory(InventoryType type, params object[] args)
            => type switch
            {
                _ => new Inventory()
            };
        
        public enum InventoryType
        {
            Base,
            
        }
    }
}