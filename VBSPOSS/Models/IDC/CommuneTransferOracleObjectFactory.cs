using Oracle.ManagedDataAccess.Types;

namespace VBSPOSS.Models.IDC
{
    [OracleCustomTypeMapping(
        "VBSPOSS.OBJ_COMMUNE_TRANSFER")]
    public class CommuneTransferOracleObjectFactory :
        IOracleCustomTypeFactory
    {
        public IOracleCustomType CreateObject()
        {
            return new CommuneTransferOracleObject();
        }
    }
}