namespace Boycott.Mapper {
    using System.Collections.Generic;

    public class Synchronizator {
        private bool initialized = false;
        private List<MapperComparer> tables;

        public Synchronizator() {
        }

        public List<MapperComparer> Tables {
            get { return tables; }
        }

        public bool DatabseExists {
            get { return Configuration.DatabaseProvider.DatabaseExists(Configuration.DatabaseProvider.Database); }
        }

        public void Initialize() {
            initialized = true;
            Configuration.DatabaseProvider.Recycle();
            var types = ObjectMapper.GetTypes();
            tables = new List<MapperComparer>();
            foreach (var type in types) {
                tables.Add(new MapperComparer(type));
            }
        }

        protected void EnsureInitialize() {
            if (initialized)
                return;
            Initialize();
        }

        public bool Check() {
            EnsureInitialize();
            foreach (var item in tables) {
                if (item.Check())
                    return true;
            }
            return false;
        }

        public bool Sync() {
            EnsureInitialize();
            var sync = false;
            foreach (var item in tables) {
                if (item.Sync())
                    sync = true;
            }
            return sync;
        }
    }
}
