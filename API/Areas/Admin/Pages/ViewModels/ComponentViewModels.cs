using System;
using System.Collections.Generic;

namespace NewsletterApp.API.Areas.Admin.Pages.ViewModels
{
    /// <summary>
    /// ViewModel for PageHeader component
    /// Provides strongly-typed model instead of dynamic
    /// </summary>
    public class PageHeaderViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Icon { get; set; } = "fas fa-list";
        public List<ActionButtonViewModel> Actions { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for action buttons in page header
    /// </summary>
    public class ActionButtonViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Style { get; set; } = "primary"; // primary, success, danger, warning, info
        public string? Icon { get; set; }
        public bool IsOutline { get; set; } = false;
        public string? CssClass { get; set; }
    }

    /// <summary>
    /// ViewModel for FilterBar component
    /// </summary>
    public class FilterBarViewModel
    {
        public string? SearchTerm { get; set; }
        public string? SearchPlaceholder { get; set; } = "Search...";
        public List<FilterOptionViewModel> TypeOptions { get; set; } = new();
        public List<FilterOptionViewModel> InterestOptions { get; set; } = new();
        public List<FilterOptionViewModel> StatusOptions { get; set; } = new();
        public bool ShowStatus { get; set; } = false;
        public bool ShowTypes { get; set; } = true;
        public bool ShowInterests { get; set; } = true;
    }

    /// <summary>
    /// ViewModel for filter dropdown option
    /// </summary>
    public class FilterOptionViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public FilterOptionViewModel() { }
        public FilterOptionViewModel(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }

    /// <summary>
    /// ViewModel for LoadingSpinner component
    /// </summary>
    public class LoadingSpinnerViewModel
    {
        public SpinnerSize Size { get; set; } = SpinnerSize.Medium;
        public string? Text { get; set; } = "Loading...";
    }

    /// <summary>
    /// Enum for spinner sizes
    /// </summary>
    public enum SpinnerSize
    {
        Small,
        Medium,
        Large
    }

    /// <summary>
    /// ViewModel for Modal dialogs (delete, confirm, etc.)
    /// </summary>
    public class ModalViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string Message { get; set; } = string.Empty;
        public ModalType Type { get; set; } = ModalType.Confirm;
        public string? Icon { get; set; }
        public ModalButtonViewModel PrimaryButton { get; set; } = new();
        public ModalButtonViewModel? SecondaryButton { get; set; }
    }

    /// <summary>
    /// ViewModel for modal buttons
    /// </summary>
    public class ModalButtonViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Style { get; set; } = "primary"; // primary, danger, warning
        public string? OnClick { get; set; }
        public string? FormId { get; set; }
        public bool IsSubmit { get; set; } = false;
    }

    /// <summary>
    /// Enum for modal types
    /// </summary>
    public enum ModalType
    {
        Confirm,
        Warning,
        Danger,
        Success,
        Info
    }

    /// <summary>
    /// ViewModel for Form components
    /// </summary>
    public class FormFieldViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Placeholder { get; set; }
        public FormFieldType Type { get; set; } = FormFieldType.Text;
        public bool Required { get; set; }
        public string? HelpText { get; set; }
        public string? CssClass { get; set; }
        public List<FormOptionViewModel> Options { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for form field options (select, radio, checkbox)
    /// </summary>
    public class FormOptionViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Selected { get; set; } = false;

        public FormOptionViewModel() { }
        public FormOptionViewModel(string label, string value, bool selected = false)
        {
            Label = label;
            Value = value;
            Selected = selected;
        }
    }

    /// <summary>
    /// Enum for form field types
    /// </summary>
    public enum FormFieldType
    {
        Text,
        Email,
        Password,
        Number,
        Date,
        Textarea,
        Select,
        Checkbox,
        Radio,
        Hidden,
        File
    }

    /// <summary>
    /// ViewModel for Form Button
    /// </summary>
    public class FormButtonViewModel
    {
        public string Label { get; set; } = string.Empty;
        public ButtonType Type { get; set; } = ButtonType.Primary;
        public ButtonSize Size { get; set; } = ButtonSize.Medium;
        public string? Icon { get; set; }
        public bool IsLoading { get; set; } = false;
        public string? OnClick { get; set; }
        public bool IsDisabled { get; set; } = false;
        public bool IsOutline { get; set; } = false;
        public string? CssClass { get; set; }
    }

    /// <summary>
    /// Enum for button types
    /// </summary>
    public enum ButtonType
    {
        Primary,
        Secondary,
        Success,
        Danger,
        Warning,
        Info,
        Light,
        Dark
    }

    /// <summary>
    /// Enum for button sizes
    /// </summary>
    public enum ButtonSize
    {
        Small,
        Medium,
        Large
    }

    /// <summary>
    /// ViewModel for complete form with multiple fields
    /// </summary>
    public class FormViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string Method { get; set; } = "post";
        public string? Action { get; set; }
        public List<FormFieldViewModel> Fields { get; set; } = new();
        public List<FormButtonViewModel> Buttons { get; set; } = new();
        public string? CssClass { get; set; }
        public Dictionary<string, string>? HiddenFields { get; set; }
    }

    /// <summary>
    /// ViewModel for data table with pagination and actions
    /// </summary>
    public class DataTableViewModel
    {
        public List<DataTableHeaderViewModel> Headers { get; set; } = new();
        public List<DataTableRowViewModel> Rows { get; set; } = new();
        public List<TableActionViewModel> RowActions { get; set; } = new();
        public string? CssClass { get; set; }
    }

    /// <summary>
    /// ViewModel for data table column header
    /// </summary>
    public class DataTableHeaderViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string? CssClass { get; set; }
    }

    /// <summary>
    /// ViewModel for data table row
    /// </summary>
    public class DataTableRowViewModel
    {
        public List<DataTableCellViewModel> Cells { get; set; } = new();
        public List<TableActionViewModel> Actions { get; set; } = new();
        public string? CssClass { get; set; }
    }

    /// <summary>
    /// ViewModel for data table cell
    /// </summary>
    public class DataTableCellViewModel
    {
        public string Value { get; set; } = string.Empty;
        public string? CssClass { get; set; }
        public bool IsHtml { get; set; } = false;
    }

    /// <summary>
    /// ViewModel for table row actions (edit, delete, etc.)
    /// </summary>
    public class TableActionViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Style { get; set; } = "primary"; // primary, success, danger, warning, info, secondary
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public string? OnClick { get; set; }
        public string? ModalTarget { get; set; } // For triggering modals
    }

    /// <summary>
    /// ViewModel for alert messages (instead of TempData)
    /// Optional: for more control over alerts in views
    /// </summary>
    public class AlertViewModel
    {
        public string Message { get; set; } = string.Empty;
        public AlertType Type { get; set; } = AlertType.Info;
        public string? Icon { get; set; }
        public bool Dismissible { get; set; } = true;
    }

    /// <summary>
    /// Enum for alert types
    /// </summary>
    public enum AlertType
    {
        Success,
        Error,
        Warning,
        Info
    }

    /// <summary>
    /// ViewModel for Pagination component
    /// </summary>
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;
        public string PageParameterName { get; set; } = "pageNumber";
        public List<int> PageSizes { get; set; } = new() { 10, 25, 50, 100 };
        public int? SelectedPageSize { get; set; } = 10;
    }
}
