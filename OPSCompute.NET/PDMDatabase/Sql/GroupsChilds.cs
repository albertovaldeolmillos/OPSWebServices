namespace PDMDatabase.Sql
{
    public static class GroupsChilds
    {
        public static readonly string Select = @"select  CGRP_ID, CGRP_TYPE, CGRP_CHILD, CGRP_ORDER  
                                                from     GROUPS_CHILDS, GROUPS 
                                                where    CGRP_VALID = 1 and CGRP_DELETED = 0 and 
                                                         GRP_ID=CGRP_ID AND GRP_DGRP_ID!=5 
                                                order by CGRP_CHILD";
    }
}
