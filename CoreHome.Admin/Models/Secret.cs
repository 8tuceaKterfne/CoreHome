using MemoryPack;

namespace CoreHome.Admin.Models
{
    [MemoryPackable]
    public partial class Secret
    {
        // ��ʼ������
        public string IV { get; set; }

        // ��Կ
        public string Key { get; set; }
    }
}