using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace PasswordChange.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            using (var mainCatalog = new AggregateCatalog())
            {
                mainCatalog.Catalogs.Add(new AssemblyCatalog(GetType().Assembly));

                using (var container = new CompositionContainer(mainCatalog))
                {
                    container.ComposeParts(this);
                }
            }
        }

        [Import]
        public PasswordChangeViewModel PasswordChangeViewModel { get; set; }
    }
}
