/**
 * Workflow processing information to be sent back to front end
 * (1) PDF File Name
 * (2) Record Number
 * */
namespace fldAsuranceApi.Models
{
    public class WorkflowInfoModel
    {
        public string fileName { get; set; } = "";
        public string fullFileName { get; set; } = "";
        public string filePath { get; set; } = "";
        public int recordID { get; set; } = 1;
    }
}