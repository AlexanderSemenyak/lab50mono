<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<script runat="server">
    private bool? Value {
        get {
            if (ViewData.Model == null) {
                return null;
            }
            return Convert.ToBoolean(ViewData.Model, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
</script>
<% if (ViewData.ModelMetadata.IsNullableValueType) { %>
    <select class="list-box tri-state" disabled="disabled">
        <option value="" <%= Value.HasValue ? "" : "selected='selected'" %>>Not Set</option>
        <option value="true" <%= Value.HasValue && Value.Value ? "selected='selected'" : "" %>>True</option>
        <option value="false" <%= Value.HasValue && !Value.Value ? "selected='selected'" : "" %>>False</option>
    </select>
<% } else { %>
    <input type="checkbox" class="check-box" disabled="disabled" <%= Value.HasValue && Value.Value ? "checked='checked'" : "" %> />
<% } %>