using PDMHelpers;
using System;

namespace PDMCompute
{
    public enum PruneFase {
        NO_FASE = 0,
        FIRST = 1,
        SECOND = 2,
        THIRD = 3
    }

    public class CM1GroupsTree
    {
        public const int TREE_UNDEFINED_NODE = -1;
        public const int TREE_MAX_NODES	= 2000;
        public const int TREE_MAX_GROUPS = 500;

        // AddGroup function return values
        public const int ADD_TREE_ERR = 0;
        public const int ADD_TREE_OK = 1;
        public const int ADD_TREE_WAS = -1;

        // Prune fase validated
        public const int PRUNE_NO_FASE = 0;
        public const int PRUNE_FIRST = 1;
        public const int PRUNE_SECOND = 2;
        public const int PRUNE_THIRD = 3;

        private int m_nNodeNum;                                 // Number of nodes of the tree
        CM1Node[] m_Nodes = new CM1Node[TREE_MAX_NODES];        // Node array

        // Groups
        int m_nGroupNum;                                        // Number of groups of the tree 
        CM1Group[] m_Groups = new CM1Group[TREE_MAX_GROUPS];    // Group array

        //Trace
        ITraceable trace;

        public CM1GroupsTree(ILoggerManager loggerManager)
        {
            this.trace = loggerManager.CreateTracer(GetType());
            Init();
        }

        public void Init()
        {
            m_nNodeNum = 0;
            m_nGroupNum = 0;

            for (int i = 0; i < TREE_MAX_NODES; i++)
            {
                m_Nodes[i] = new CM1Node();
            }

            for (int i = 0; i < TREE_MAX_GROUPS; i++)
            {
                m_Groups[i] = new CM1Group();
            }
        }

        public bool Reset()
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::Reset");
            bool fnResult = true;

            try
            {
                m_nNodeNum = 0;
                m_nGroupNum = 0;
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        // Nodes
        public int GetNodeNum() { return m_nNodeNum; }
        public CM1Node[] GetNodes() { return m_Nodes; }
        public CM1Node GetFirstNodeFromIds(long lGrpId, long lGrpChId = GlobalDefs.DEF_UNDEFINED_VALUE)
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::GetFirstNodeFromIds");
            bool fnResult = true;

            CM1Node nRdo = null;

            try
            {
                if (lGrpId == GlobalDefs.DEF_UNDEFINED_VALUE)
                    throw new ArgumentNullException(nameof(lGrpId), "lGrpId == DEF_UNDEFINED_VALUE");

                for (int i = 0; i < m_nNodeNum; i++)
                {
                    if (m_Nodes[i].GrpId == lGrpId &&                    // GrpId
                            ((lGrpChId != GlobalDefs.DEF_UNDEFINED_VALUE &&            // GrpChildId is defined
                                    m_Nodes[i].GrpChild != null &&
                                    m_Nodes[i].GrpChild.GetGrpId() == lGrpChId)
                                ||
                                (lGrpChId == GlobalDefs.DEF_UNDEFINED_VALUE &&         // GrpChildId is not defined
                                    m_Nodes[i].GrpChild == null)
                            )
                        )
                    {
                        nRdo = m_Nodes[i];
                        break;
                    }
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            if (!fnResult)
            {
                nRdo = null;
            }

            return nRdo;
        }
        public int AddNode(long lGrpId, long lGrpChildId = GlobalDefs.DEF_UNDEFINED_VALUE, int nNumCnstr = 0)
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::AddNode");

            bool fnResult = true;
            int nRdo = ADD_TREE_OK;

            try
            {
                if (GetFirstNodeFromIds(lGrpId, lGrpChildId) != null)
                {
                    nRdo = ADD_TREE_WAS;
                }
                else
                {
                    m_Nodes[m_nNodeNum].Init();
                    m_Nodes[m_nNodeNum].GrpId = lGrpId;
                    m_Nodes[m_nNodeNum].Grp = GetGroupFromGrpId(lGrpId);
                    m_nGroupNum++;

                    // Fill Group child info
                    if (lGrpChildId != GlobalDefs.DEF_UNDEFINED_VALUE) // It's a child!
                    {
                        // Verify parent existence
                        CM1Node pParentNode = GetFirstNodeFromIds(lGrpId);
                        if (pParentNode == null)
                            throw new InvalidOperationException("Trying to add child node without parent node");

                        m_Nodes[m_nNodeNum].GrpChild = GetGroupFromGrpId(lGrpChildId);
                        m_Nodes[m_nNodeNum].ChildsNum = 0;
                        m_Nodes[m_nNodeNum].CnstrNum = 0;

                        // Number of childs++
                        pParentNode.ChildsNum++;
                    }
                    else // Not Child
                    {
                        m_Nodes[m_nNodeNum].GrpChild = null;
                        m_Nodes[m_nNodeNum].ChildsNum = 0;
                        m_Nodes[m_nNodeNum].CnstrNum = nNumCnstr;
                    }

                    m_nNodeNum++;   // Inc node counter

                    nRdo = ADD_TREE_OK;
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            if (!fnResult)
            {
                nRdo = ADD_TREE_ERR;
            }

            return nRdo;
        }

        // Groups
        public int GetGroupNum() { return m_nGroupNum; }
        public CM1Group[] GetGroups() { return m_Groups; }
        public CM1Group GetGroupFromGrpId(long lGrpId)
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::GetGroupFromGrpId");
            bool fnResult = true;

            CM1Group nRdo = null;

            try
            {
                if (lGrpId == GlobalDefs.DEF_UNDEFINED_VALUE)
                    throw new ArgumentNullException(nameof(lGrpId), "lGrpId == DEF_UNDEFINED_VALUE");

                for (int i = 0; i < m_nGroupNum; i++)
                {
                    if (m_Groups[i].GetGrpId() == lGrpId)
                    {
                        nRdo = m_Groups[i];
                        break;
                    }
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            if (!fnResult)
            {
                nRdo = null;
            }

            return nRdo;
        }
        public int AddGroup(long lGrpId, long lGrpTypeId) {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::AddGroup");

            bool fnResult = true;
            int  nRdo = ADD_TREE_OK;

            try
            {
                if (GetGroupFromGrpId(lGrpId) != null)
                {
                    nRdo = ADD_TREE_WAS;
                }
                else
                {
                    m_Groups[m_nGroupNum] = new CM1Group();
                    m_Groups[m_nGroupNum].Init();
                    m_Groups[m_nGroupNum].SetGrpId(lGrpId);
                    m_Groups[m_nGroupNum].SetGrpTypeId(lGrpTypeId);
                    m_nGroupNum++;

                    nRdo = AddNode(lGrpId);
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            if (!fnResult) {
                nRdo = ADD_TREE_ERR;
            }

            return nRdo;
        }
        public CM1Group GetGroupParent(long lGrpId)
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::GetGroupParent");
            bool fnResult = true;

            CM1Group nRdo = null;

            try
            {
                if (lGrpId == GlobalDefs.DEF_UNDEFINED_VALUE)
                    throw new ArgumentNullException(nameof(lGrpId), "lGrpId == DEF_UNDEFINED_VALUE");

                for (int i = 0; i < m_nNodeNum; i++)
                {
                    if (m_Nodes[i].GrpChild != null && m_Nodes[i].GrpChild.GetGrpId() == lGrpId)
                    {
                        return GetGroupFromGrpId(m_Nodes[i].GrpId);
                    }
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            if (!fnResult)
            {
                nRdo = null;
            }

            return nRdo;
        }
        public bool IsGroupInTree(long lTreeGrpId, long lGrpId)
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::IsGroupInTree");

            CM1Group pGrp = null;
            bool bRes = false;
            long lActGrpId;

            try
            {
                if (lGrpId == GlobalDefs.DEF_UNDEFINED_VALUE)
                    throw new ArgumentNullException(nameof(lGrpId), "lGrpId == DEF_UNDEFINED_VALUE");

                if (lTreeGrpId == GlobalDefs.DEF_UNDEFINED_VALUE)
                    throw new ArgumentNullException(nameof(lTreeGrpId), "lTreeGrpId == DEF_UNDEFINED_VALUE");

                lActGrpId = lGrpId;
                if (lTreeGrpId == lActGrpId)
                {
                    bRes = true;
                }
                else
                {
                    do
                    {
                        pGrp = GetGroupParent(lActGrpId);
                        if (pGrp != null)
                        {
                            lActGrpId = pGrp.GetGrpId();
                            if (lTreeGrpId == lActGrpId)
                            {
                                bRes = true;
                                break;
                            }

                        }
                    }
                    while (pGrp != null);
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
            }

            return bRes;
        }

        // Constraints
        public bool MergeOrAddConstraint(CM1Group pGrp, long lCnstrDef, float fValue)
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::MergeOrAddConstraint");
            bool fnResult = true;

            try
            {
                if (pGrp == null)
                    throw new ArgumentNullException(nameof(pGrp), "pGrp is NULL");

                // TODO: Best control
                // If MergeConstraint fails I suppose that previosly this type of constraint
                // was not set ... so I add it.
                float? result = null;
                if (!pGrp.MergeConstraint(lCnstrDef, fValue, ref result))
                {
                    if (pGrp.AddConstraint(lCnstrDef, fValue))
                    {
                        CM1Node pNode = GetFirstNodeFromIds(pGrp.GetGrpId());
                        if (pNode != null)
                            pNode.CnstrNum++;
                    }
                    else
                        fnResult = false;
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        // Prune
        // Fase 1: Save childs when parent has constraints
        public bool PruneFase1(long lGrpId = GlobalDefs.DEF_UNDEFINED_VALUE) {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::PruneFase1");
            bool fnResult = true;

            try
            {
                for (int j = 0; j < GetNodeNum(); j++)
                {
                    CM1Node pNode = m_Nodes[j];

                    if (pNode == null)
                        continue;

                    if (lGrpId == GlobalDefs.DEF_UNDEFINED_VALUE &&            // first time
                        pNode.GrpChild == null &&           // all leaves
                        pNode.CnstrNum != 0 &&          // with constraints
                        pNode.PruneFase != PruneFase.FIRST)     // not pruned before
                    {
                        pNode.PruneFase = PruneFase.FIRST;      // set the prune level
                        if (pNode.GrpId == GlobalDefs.DEF_UNDEFINED_VALUE)
                            throw new InvalidOperationException("Error pNode->m_lGrpId == DEF_UNDEFINED_VALUE");

                        if (!PruneFase1(pNode.GrpId))       // look for childs
                            throw new InvalidOperationException("Error in inner call");
                    }
                    else                                        // I look for a group (and it's childs)
                    {
                        if (pNode.GrpId == lGrpId &&    // It is the group
                            pNode.PruneFase != PruneFase.FIRST) // not pruned before
                        {
                            if (pNode.GrpChild == null)         // It is a leave, set and continue;
                                pNode.PruneFase = PruneFase.FIRST;
                            else // Has childs ... look for them
                            {
                                if (pNode.GrpChild.GetGrpId() ==  GlobalDefs.DEF_UNDEFINED_VALUE)
                                    throw new InvalidOperationException("Error pNode->m_pGrpChild->GetGrpId() == DEF_UNDEFINED_VALUE");
                                if (!PruneFase1(pNode.GrpChild.GetGrpId()))
                                    throw new InvalidOperationException("Error in inner call");
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        // Fase 2: Save childs/parents of operation group
        public bool PruneFase2(long lGrpId)
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::PruneFase2");
            bool fnResult = true;

            try
            {
                if (lGrpId == GlobalDefs.DEF_UNDEFINED_VALUE)
                    throw new ArgumentNullException(nameof(lGrpId), "lGrpId == DEF_UNDEFINED_VALUE");

                // Look for the group
                CM1Node pNode = GetFirstNodeFromIds(lGrpId);
                if (pNode == null)
                    throw new InvalidOperationException("Can't find the group");

                if (pNode.PruneFase == PruneFase.FIRST)
                {

                    pNode.PruneFase = PruneFase.SECOND;

                    // Look for parents and childs
                    for (int j = 0; j < GetNodeNum(); j++)
                    {
                        CM1Node pLoopNode = m_Nodes[j];

                        if (pLoopNode == null ||       // No node or
                            pLoopNode == pNode) // node-group
                            continue;

                        if (pLoopNode.GrpId == lGrpId && pLoopNode.GrpChild != null)        // Child
                        {
                            if (!PruneFase2(pLoopNode.GrpChild.GetGrpId()))
                            {
                                throw new InvalidOperationException("Error in PruneFase2(pLoopNode->m_pGrpChild->GetGrpId()");
                            }
                        }


                        if (pLoopNode.GrpChild != null && pLoopNode.GrpChild.GetGrpId() == lGrpId) // Parent
                        {
                            if (!PruneFase2(pLoopNode.GrpId))
                            {
                                throw new InvalidOperationException("Error in PruneFase2(pLoopNode->m_lGrpId)");
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        // Fase 3: Compact Tree
        public bool PruneFase3()
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::PruneFase3");
            bool fnResult = true;

            try
            {
                CM1Node[] dummyNodes = new CM1Node[CM1GroupsTree.TREE_MAX_NODES];
                int nNewNodeNum = 0;

                // Add node-base
                int j;
                for (j = 0; j < GetNodeNum(); j++)
                {
                    CM1Node pNode = m_Nodes[j];

                    if (pNode == null)
                        continue;

                    // Node is valid?
                    if (pNode.GrpChild == null && pNode.PruneFase == PruneFase.SECOND)
                    {
                        // Yes, copy the node
                        CM1Node pNdDest = dummyNodes[nNewNodeNum++];

                        if (pNdDest != null) {
                            pNdDest.Copy(pNode);
                        }
                    }

                    if (pNode.GrpChild == null)
                    {
                        pNode.Init();
                    }
                }

                // Add the childs (if parent and child are in the new node-base set)
                for (j = 0; j < GetNodeNum(); j++)
                {
                    CM1Node pNode = m_Nodes[j];

                    if (pNode == null)
                        continue;

                    // Not node-base, only child relationship
                    if (pNode.GrpChild != null)
                    {
                        // ... if parent and child are in the new node-base set ...
                        bool bFindParent = false;
                        bool bFindChild = false;
                        for (int i = 0; i < nNewNodeNum; i++)
                        {
                            CM1Node pNdDest = dummyNodes[i];

                            if (pNdDest != null && pNdDest.GrpChild == null && pNdDest.GrpId == pNode.GrpId)
                            {
                                bFindParent = true;
                            }


                            if (pNdDest != null && pNdDest.GrpChild == null && pNdDest.GrpId == pNode.GrpChild.GetGrpId())
                            {
                                bFindChild = true;
                            }
                        }

                        if (bFindParent && bFindChild) // Yes, copy the node
                        {
                            CM1Node pNdDest = dummyNodes[nNewNodeNum++];

                            if (pNdDest != null)
                            {
                                pNdDest.Copy(pNode);
                            }
                        }
                    }

                    pNode.Init();
                }

                m_nNodeNum = 0;

                // First we add base nodes
                for (j = 0; j < nNewNodeNum; j++)
                {
                    CM1Node pNdSrc = dummyNodes[j];

                    if (pNdSrc == null)  continue;

                    if (pNdSrc.GrpChild == null)
                    {
                        AddNode(pNdSrc.GrpId, GlobalDefs.DEF_UNDEFINED_VALUE, pNdSrc.CnstrNum);
                    }
                }

                // Second we add child nodes
                for (j = 0; j < nNewNodeNum; j++)
                {
                    CM1Node pNdSrc = dummyNodes[j];

                    if (pNdSrc == null)
                        continue;

                    if (pNdSrc.GrpChild != null)
                    {
                        AddNode(pNdSrc.GrpId, pNdSrc.GrpChild.GetGrpId());
                    }
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public void SetTracerEnabled(bool enabled)
        {
            if (trace != null)
            {
                trace.Enabled = enabled;
            }
        }

        public bool AmpliationIsAllowed(long groupId, ref bool ampliationIsAllowed)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupsTree::AmpliationIsAllowed");
            bool fnResult = true;

            try
            {
                CM1Group group = GetGroupFromGrpId(groupId);
                float isAllowed = GlobalDefs.DEF_UNDEFINED_VALUE;
                group.GetConstraint(CM1Constraint.CNSTR_AMP_ALLOW, ref isAllowed);

                if (isAllowed == CM1Constraint.CNSTR_UNDEFINED) {
                    isAllowed = 1;
                }

                ampliationIsAllowed = isAllowed != 0;

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        public bool GetBranchMinMoney(long groupId, ref long money, bool isComputeEx1 = false)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupsTree::GetBranchMinMoney");
            bool fnResult = true;

            try
            {
                // Get group object of the group of the operation to evaluate
                CM1Group pGrp = GetGroupFromGrpId(groupId);

                if (pGrp == null)
                {
                    throw  new InvalidOperationException("Could not obtain group of current operation");
                }

                long lGrpAccMoney = 0;
                long lGrpMinMoney = 0;
                money = -1;

                while (pGrp != null)
                {
                    lGrpAccMoney = (isComputeEx1) ? pGrp.GetEfecAccMoney() : pGrp.GetAccMoney();
                    lGrpMinMoney = pGrp.GetMinMoney();

                    if (lGrpMinMoney == GlobalDefs.DEF_UNDEFINED_VALUE || lGrpMinMoney < lGrpAccMoney)
                    {
                        lGrpMinMoney = 0;
                    }
                    else
                    {
                        lGrpMinMoney -= lGrpAccMoney;
                    }

                    money = Math.Max(lGrpMinMoney, money);

                    pGrp = GetGroupParent(pGrp.GetGrpId());
                }
                trace.Write(TraceLevel.Info, $"Min. money for branch of group {groupId}: {money}");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        public bool GetMinValueToChargeInRefund(long groupId, ref long minMoney)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupsTree::GetMinValueToChargeInRefund");
            bool fnResult = true;

            try
            {
                // Get group object of the group of the operation to evaluate
                CM1Group pGrp = GetGroupFromGrpId(groupId);

                if (pGrp == null)
                {
                    throw new InvalidOperationException("Could not obtain group of current operation");
                }

                long lGrpMinValueToCharge = 0;
                long lMinValueToCharge = 0;
                minMoney = -1;

                while (pGrp != null)
                {
                    lGrpMinValueToCharge = pGrp.GetMinValueToChargeInRefund();

                    if (lGrpMinValueToCharge == GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        lMinValueToCharge = 0;
                    }
                    else
                    {
                        lMinValueToCharge = lGrpMinValueToCharge;
                    }

                    minMoney = Math.Max(lMinValueToCharge, minMoney);

                    pGrp = GetGroupParent(pGrp.GetGrpId());
                }
                trace.Write(TraceLevel.Info, $"Min. money to charge in refunds {groupId}: {minMoney}");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        // Trace
        public void TraceFullTree() {

            trace.Write(TraceLevel.Debug, "CM1GroupsTree::TraceFullTree");
            try
            {
                trace?.Write(TraceLevel.Debug, $"#Groups: {GetGroupNum()} , #Nodes:{GetNodeNum()}");
                trace?.Write(TraceLevel.Debug, "Groups [  Group  | Type | NumChilds | NumConstraits | PruneFase ]   Childs {  Group  | ChildGroup }:");
                trace?.Write(TraceLevel.Debug, "Constraints ( Type |   Value   ):");

                for (int i = 0; i < GetGroupNum(); i++)
                {
                    CM1Group pGrp = m_Groups[i];

                    if (pGrp == null)
                        continue;

                // For each group have to be a node
                CM1Node pNode = GetFirstNodeFromIds(pGrp.GetGrpId());
                if (pNode != null)
                {
                        trace?.Write(TraceLevel.Debug, $"       [ {pGrp.GetGrpId(),7} | {pGrp.GetGrpTypeId(),4} | {pNode.ChildsNum,9} | {pNode.CnstrNum,13} | {pNode.PruneFase} ]\t");

                    // A node can have constraints
                    CM1Constraint[] pCnstr = pGrp.GetConstraints();
                    for (int k = 0; k < CM1Constraint.CNSTR_NUM; k++)
                    {
                        if (!pCnstr[k].IsValid())
                            continue;

                        trace?.Write(TraceLevel.Debug,  $"( {pCnstr[k].TypeId, 4} | %{pCnstr[k].Value},4)");
                    }

                    // A node can have childs
                    for (int j = 0; j < GetNodeNum(); j++)
                    {
                        pNode = m_Nodes[j];

                        if (pNode == null || pNode.GrpChild == null || pNode.GrpId != pGrp.GetGrpId())
                            continue;

                        trace?.Write(TraceLevel.Debug, $" ( {pNode.GrpId,7} | {pNode.GrpChild.GetGrpId(),10} )\t");
                    }


                    ////sRdo += _T("\n");
                    //trace?.Write(TraceLevel.Info, (LPCTSTR)strRdo);

                    //if (!strCnstr.IsEmpty())
                    //{
                    //    /*
                    //    sRdo += _T("\t");
                    //    sRdo += sCnstr;
                    //    sRdo += _T("\n");
                    //    */
                    //    trace.Write(TRACE_M1_LEVEL, _T("\t%s"), (LPCTSTR)strCnstr);
                    //}
                }
            }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }
        }
        public void TraceBranchM1ComputeEx0(long lGrpId)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupsTree::TraceBranchM1ComputeEx0");
            try
            {

                trace.Write(TraceLevel.Debug, $"Tracing branch of group: {lGrpId}");
                trace.Write(TraceLevel.Debug, $"[  Group  | Type | NumChilds | NumConstraits | PruneFase | Money | Minutes ]");
                CM1Group pGrp = GetGroupFromGrpId(lGrpId);

                while (pGrp != null)
                {
                    // For each group have to be a node
                    CM1Node pNode = GetFirstNodeFromIds(pGrp.GetGrpId());

                    if (pNode != null)
                    {
                        trace.Write(TraceLevel.Debug, $"[ {pGrp.GetGrpId(),-7} | {pGrp.GetGrpTypeId(),-4} | {pNode.ChildsNum, 9} | {pNode.CnstrNum, -13} | {pNode.PruneFase,-9} | {pGrp.GetAccMoney(),-5} | {pGrp.GetAccMinutes(),-7} ]");
                    }
                    pGrp = GetGroupParent(pGrp.GetGrpId());
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }

        }
        public void TraceBranchM1ComputeEx1(long lGrpId)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupsTree::TraceBranchM1ComputeEx1");
            try
            {

                trace.Write(TraceLevel.Debug, $"Tracing branch of group: {lGrpId}");
                trace.Write(TraceLevel.Debug, $"[  Group  | Type | NumChilds | NumConstraits | PruneFase | Money | Minutes ]");
                CM1Group pGrp = GetGroupFromGrpId(lGrpId);

                while (pGrp != null)
                {
                    // For each group have to be a node
                    CM1Node pNode = GetFirstNodeFromIds(pGrp.GetGrpId());

                    if (pNode != null)
                    {
                        trace.Write(TraceLevel.Debug, $"[ {pGrp.GetGrpId(),-7} | {pGrp.GetGrpTypeId(),-4} | {pNode.ChildsNum,9} | {pNode.CnstrNum,-13} | {pNode.PruneFase,-9} | {pGrp.GetAccMoney(),-5} | {pGrp.GetAccMinutes(),-7} ]");
                    }
                    pGrp = GetGroupParent(pGrp.GetGrpId());
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
            }
        }
        public void SetTrace(ITraceable pTrace)
        {
            trace = pTrace;
        }

        public bool Copy(CM1GroupsTree pSrcTree) {
            trace?.Write(TraceLevel.Debug, "CM1GroupsTree::Copy");
            bool fnResult = true;

            try
            {
                trace = pSrcTree.trace;
                m_nNodeNum = pSrcTree.m_nNodeNum;
                m_nGroupNum = pSrcTree.m_nGroupNum;

                int i = 0;
                for (i = 0; i < m_nGroupNum; i++)
                {
                    m_Groups[i].Init();
                    m_Groups[i].Copy(pSrcTree.m_Groups[i]);
                }

                long lGrpId;
                for (i = 0; i < m_nNodeNum; i++)
                {
                    m_Nodes[i].Init();
                    m_Nodes[i].Copy(pSrcTree.m_Nodes[i]);
                    if (pSrcTree.m_Nodes[i].Grp != null)
                    {
                        lGrpId = pSrcTree.m_Nodes[i].Grp.GetGrpId();
                        m_Nodes[i].Grp = GetGroupFromGrpId(lGrpId);
                    }
                    if (pSrcTree.m_Nodes[i].GrpChild != null)
                    {
                        lGrpId = pSrcTree.m_Nodes[i].GrpChild.GetGrpId();
                        m_Nodes[i].GrpChild = GetGroupFromGrpId(lGrpId);
                    }

                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        // OPERATIONS
        /// <summary>
        ///	Returns the maximum minutes that a user can spend in a branch without overflowing the maximum money constraint in any group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="minutes"></param>
        /// <returns>bool</returns>
        public bool GetBranchMaxAvailableMinutes(long groupId, ref long minutes, bool isComputeEx1 = false)
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupTree::GetBranchMaxAvailableMoney");
            bool fnResult = true;

            try
            {
                CM1Group treeGroup = GetGroupFromGrpId(groupId);
                if (treeGroup == null)
                {
                    throw new ArgumentNullException(nameof(groupId), "Could not obtain group of current operation");
                }

                long lGrpAccMinutes = 0;
                long lGrpMaxMinutes = 0;

                minutes = 999999999;

                while (treeGroup != null)
                {
                    if (isComputeEx1) {
                        lGrpAccMinutes = treeGroup.GetRealAccMinutes();
                    }
                    else {
                        lGrpAccMinutes = treeGroup.GetAccMinutes();
                    }
                    
                    lGrpMaxMinutes = treeGroup.GetMaxMinutes();

                    if (lGrpMaxMinutes != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        minutes = Math.Min(lGrpMaxMinutes - lGrpAccMinutes, minutes);
                    }

                    treeGroup = GetGroupParent(treeGroup.GetGrpId());
                }

                trace?.Write(TraceLevel.Info, $"Minutes left for branch of group {groupId}: {minutes}");

            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        /// <summary>
        ///	Returns the maximum money that a user can spend in a branch without overflowing the maximum money constraint in any group
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="groupId"></param>
        /// <param name="money"></param>
        /// <returns>bool</returns>
        public bool GetBranchMaxAvailableMoney(long groupId, ref long money, bool isComputeEx1 = false)
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupTree::GetBranchMaxAvailableMoney");
            bool fnResult = true;

            try
            {
                CM1Group treeGroup = GetGroupFromGrpId(groupId);
                if (treeGroup == null)
                {
                    throw new ArgumentNullException(nameof(groupId), "Could not obtain group of current operation");
                }

                long lGrpAccMoney = 0;
                long lGrpMaxMoney = 0;

                money = 999999999;

                while (treeGroup != null)
                {
                    lGrpAccMoney = (isComputeEx1) ? treeGroup.GetRealAccMoney() : treeGroup.GetAccMoney();
                    lGrpMaxMoney = treeGroup.GetMaxMoney();

                    if (lGrpMaxMoney != GlobalDefs.DEF_UNDEFINED_VALUE)
                    {
                        money = Math.Min(lGrpMaxMoney - lGrpAccMoney, money);
                    }

                    treeGroup = GetGroupParent(treeGroup.GetGrpId());
                }

                trace?.Write(TraceLevel.Info, $"Money left for branch of group {groupId}: {money}");
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public bool GetBranchAccumMoney(long lGroup, ref long m_lRealAccMoney)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupTree::GetBranchAccumMoney");
            bool fnResult = true;

            try
            {
                CM1Group group = GetGroupFromGrpId(lGroup);
                if (group == null)
                {
                    throw new InvalidOperationException("Could not obtain group of current operation");
                }

                long lGrpAccMoney = 0;
                m_lRealAccMoney = 0;

                while (group != null)
                {
                    lGrpAccMoney = group.GetAccMoney();
                    m_lRealAccMoney = Math.Max(lGrpAccMoney, m_lRealAccMoney);
                    group = GetGroupParent(group.GetGrpId());
                }

                trace.Write(TraceLevel.Info, $"Money accumulate for branch of group {lGroup}: {m_lRealAccMoney}");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public bool GetBranchAccumMinutes(long lGroup, ref long m_lRealAccMinutes)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupsTree::GetBranchAccumMinutes");
            bool fnResult = true;

            try
            {
                CM1Group group = GetGroupFromGrpId(lGroup);
                if (group == null)
                {
                    throw new InvalidOperationException("Could not obtain group of current operation");
                }

                long lGrpAccMinutes = 0;
                m_lRealAccMinutes = 0;

                while (group != null)
                {
                    lGrpAccMinutes = group.GetAccMinutes();
                    m_lRealAccMinutes = Math.Max(lGrpAccMinutes, m_lRealAccMinutes);
                    group = GetGroupParent(group.GetGrpId());
                }

                trace.Write(TraceLevel.Info, $"Minutes accumulate for branch of group {lGroup}: {m_lRealAccMinutes}");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public bool GetBranchEfecAccumMinutes(long lGroup, ref long m_lRealAccMinutes)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupsTree::GetBranchAccumMinutes");
            bool fnResult = true;

            try
            {
                CM1Group group = GetGroupFromGrpId(lGroup);
                if (group == null)
                {
                    throw new InvalidOperationException("Could not obtain group of current operation");
                }

                long lGrpAccMinutes = 0;
                m_lRealAccMinutes = 0;

                while (group != null)
                {
                    lGrpAccMinutes = group.GetEfecAccMinutes();
                    m_lRealAccMinutes = Math.Max(lGrpAccMinutes, m_lRealAccMinutes);
                    group = GetGroupParent(group.GetGrpId());
                }

                trace.Write(TraceLevel.Info, $"Minutes accumulate for branch of group {lGroup}: {m_lRealAccMinutes}");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public bool GetBranchEfecAccumMoney(long lGroup, ref long m_lRealAccMoney)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupTree::GetBranchAccumMoney");
            bool fnResult = true;

            try
            {
                CM1Group group = GetGroupFromGrpId(lGroup);
                if (group == null)
                {
                    throw new InvalidOperationException("Could not obtain group of current operation");
                }

                long lGrpAccMoney = 0;
                m_lRealAccMoney = 0;

                while (group != null)
                {
                    lGrpAccMoney = group.GetEfecAccMoney();
                    m_lRealAccMoney = Math.Max(lGrpAccMoney, m_lRealAccMoney);
                    group = GetGroupParent(group.GetGrpId());
                }

                trace.Write(TraceLevel.Info, $"Money accumulate for branch of group {lGroup}: {m_lRealAccMoney}");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
    
        public bool GetBranchMaxInterdateReentry(long lGroup, ref long lMaxInDtRee)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupsTree::GetBranchMaxInterdateReentry");
            bool fnResult = true;

            try
            {
                lMaxInDtRee = GlobalDefs.DEF_UNDEFINED_VALUE;

                // Get group object of the group of the operation to evaluate
                CM1Group pGrp = GetGroupFromGrpId(lGroup);

                if (pGrp == null)
                    throw new InvalidOperationException("Could not obtain group of current operation");

                long lGrpAccInDtRee = GlobalDefs.DEF_UNDEFINED_VALUE;

                while ((pGrp != null) && (lGrpAccInDtRee == GlobalDefs.DEF_UNDEFINED_VALUE))
                {
                    lGrpAccInDtRee = pGrp.GetMaxInterdateReentry();

                    pGrp = GetGroupParent(pGrp.GetGrpId());
                }

                lMaxInDtRee = lGrpAccInDtRee;
                trace.Write(TraceLevel.Info, $"Max. Interdate for reentry for group {lGroup}: {lMaxInDtRee}");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        internal bool SetInitialIntervalOffset(long groupId, ref stINTERVAL m_stInitialIntervalOffset, bool isComputeEx1 = false)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupsTree::SetInitialIntervalOffset");
            bool fnResult = true;

            try
            {
                Guard.IsNull(m_stInitialIntervalOffset, nameof(m_stInitialIntervalOffset));


                if (isComputeEx1)
                {
                    GetBranchEfecAccumMinutes(groupId, ref m_stInitialIntervalOffset.iTime);
                    GetBranchEfecAccumMoney(groupId, ref m_stInitialIntervalOffset.iMoney);
                }
                else
                {
                    GetBranchAccumMinutes(groupId, ref m_stInitialIntervalOffset.iTime);
                    GetBranchAccumMoney(groupId, ref m_stInitialIntervalOffset.iMoney);
                }

                trace.Write(TraceLevel.Info, $"Initial offsets -> Time: {m_stInitialIntervalOffset.iTime}, Money: {m_stInitialIntervalOffset.iMoney}");
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }

        public bool ApplyCurrentOperation(long groupId, COPSDate operationDate)
        {
            trace?.Write(TraceLevel.Debug, "CM1GroupTree::ApplyCurrentOperation");
            bool fnResult = true;

            try
            {
                //m_dtOper
                CM1Group group = GetGroupFromGrpId(groupId);
                if (group == null)
                    throw new InvalidOperationException("Group of operation not found in tree");

                long groupIdTmp;
                while (group != null)
                {
                    groupIdTmp = group.GetGrpId();

                    group.SetState((long)M1GroupState.GRP_ON);
                    group.SetLastDate(operationDate);

                    group = GetGroupParent(groupIdTmp);
                    if (group == null)
                        trace?.Write(TraceLevel.Error, $"Group of operation ({groupId}) has no parents in tree");

                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
        public bool CheckHistoryState(long lGroup, ref int iHistoryRes)
        {
            trace.Write(TraceLevel.Debug, "CM1GroupTree::CheckHistoryState");
            bool fnResult = true;

            int iState;
            bool bGrpStop = true;

            try
            {
                CM1Group group = GetGroupFromGrpId(lGroup);
                if (group == null)
                {
                    throw new InvalidOperationException("Could not obtain group of current operation");
                }

                iHistoryRes = (int)M1GroupState.GRP_ON;

                while (group != null)
                {
                    iState = (int)group.GetState();
                    lGroup = group.GetGrpId();

                    bGrpStop = (bGrpStop && (iState == (int)M1GroupState.GRP_STOP));

                    if (iState == (int)M1GroupState.GRP_REE)
                    {
                        iHistoryRes = iState;
                        break;
                    }

                    group = GetGroupParent(lGroup);
                }

                if (bGrpStop)
                {
                    iHistoryRes = (int)M1GroupState.GRP_STOP;
                }

            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
    }
}