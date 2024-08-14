using System.Threading.Tasks;

namespace Red.Sys
{
    public interface IManagedMenu
    {
        public Task Close();
        public Task Open(params string[] args);
        public Task ReclaimFocusFrom(IManagedMenu menu);
        public Task SurrenderFocusTo(IManagedMenu menu);
    }
}