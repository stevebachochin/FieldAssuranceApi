// T4 code generation is enabled for model 'C:\Users\bachochs\workspace\SDSApi\SDSApi\Models\PagingParameterModelModel.edmx'. 
// To enable legacy code generation, change the value of the 'Code Generation Strategy' designer
// property to 'Legacy ObjectContext'. This property is available in the Properties Window when the model
// is open in the designer.

// If no context and entity classes have been generated, it may be because you created an empty model but
// have not yet chosen which version of Entity Framework to use. To generate a context class and entity
// classes for your model, open the model in the designer, right-click on the designer surface, and
// select 'Update Model from Database...', 'Generate Database from Model...', or 'Add Code Generation
// Item...'.
namespace fldAsuranceApi.Models
{
    public class PagingParameterModel
    {
        const int maxPageSize = 20;
        const string defaultSortOrder = "asc";

        public string _sortOrder { get; set; } = "asc";

        public string columnName { get; set; } = "CName";

        public string querySearchName { get; set; } = "CName";

        public int pageNumber { get; set; } = 1;

        public int _pageSize { get; set; } = 10;

        public int pageSize
        {

            get { return _pageSize; }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
        public string sortOrder
        {

            get { return _sortOrder; }
            set
            {
                _sortOrder = (value != "desc" ) ? defaultSortOrder : value;
            }
        }

        public string querySearch { get; set; }
    }
}