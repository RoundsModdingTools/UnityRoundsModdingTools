namespace URMT.Template.Templates {
    public interface ITemplateHandler {
        string HandleTemplate(string content);
        void InitTemplate(params object[] args);
        void AfterTemplateCompile();
    }
}
