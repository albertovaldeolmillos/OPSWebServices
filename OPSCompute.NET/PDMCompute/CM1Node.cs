namespace PDMCompute
{
    public class CM1Node
    {
        public long GrpId { get; set; }             // Group Id
        public CM1Group Grp { get; set; }           // Pointer To Group
        public CM1Group GrpChild { get; set; }      // Pointer To Child Group (is NULL if there are no childs)
        public int ChildsNum { get; set; }          // Number of childs of the group (only if m_pGrpChild = NULL)
        public int CnstrNum { get; set; }           // Number of constraints of the group (only if m_pGrpChild = NULL)
        public PruneFase PruneFase { get; set; }	// Prune fase validated (PRUNE_NO_FASE, PRUNE_FIRST, PRUNE_SECOND)

        public CM1Node()
        {
            Init();
        }

        public void Init()
        {
            GrpId = CM1GroupsTree.TREE_UNDEFINED_NODE;
            Grp = null;
            GrpChild = null;
            ChildsNum = 0;
            CnstrNum = 0;
            PruneFase = CM1GroupsTree.PRUNE_NO_FASE;
        }

        public bool Copy(CM1Node source)
        {
            if (source == null)
                return false;

            GrpId = source.GrpId;
            Grp = source.Grp;
            GrpChild = source.GrpChild;
            ChildsNum = source.ChildsNum;
            CnstrNum = source.CnstrNum;
            PruneFase = source.PruneFase;

            return true;
        }

        public static bool Copy(CM1Node destination, CM1Node source) {
            if (destination == null) {
                return false;
            }

            return destination.Copy(source);
        }
    }
}