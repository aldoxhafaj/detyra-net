using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using Microsoft.Win32;

namespace EProtokoll.Desktop;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly HttpClient _client = new();
    private string? _accessToken;
    private string _baseUrl = "https://localhost:5001";
    private string _userName = "admin";
    private string _loginStatus = "Not logged in";
    private string _letterSubject = string.Empty;
    private string _institutionName = string.Empty;
    private string _institutionExternalId = string.Empty;
    private string _institutionAddress = string.Empty;
    private string _institutionContact = string.Empty;
    private string _newUserName = string.Empty;
    private string _newUserFullName = string.Empty;
    private string _newUserRole = "Employee";
    private string _documentLetterId = string.Empty;
    private string _documentId = string.Empty;
    private string _reportOutput = string.Empty;
    private string _notificationReadId = string.Empty;
    private string _protocolYear = DateTime.UtcNow.Year.ToString();
    private string _letterActionId = string.Empty;
    private string _assignUserId = string.Empty;
    private string _responseLetterId = string.Empty;
    private string _responseMessage = string.Empty;
    private string _accessLetterId = string.Empty;
    private string _accessUserId = string.Empty;
    private string _accessDepartmentId = string.Empty;
    private string _departmentName = string.Empty;
    private string _outgoingChannel = string.Empty;
    private string _outgoingReference = string.Empty;

    public ObservableCollection<LetterDto> Letters { get; } = new();
    public ObservableCollection<InstitutionDto> Institutions { get; } = new();
    public ObservableCollection<UserDto> Users { get; } = new();
    public ObservableCollection<DocumentDto> Documents { get; } = new();
    public ObservableCollection<NotificationDto> Notifications { get; } = new();
    public ObservableCollection<ResponseDto> Responses { get; } = new();
    public ObservableCollection<AccessUserDto> AccessUsers { get; } = new();
    public ObservableCollection<AccessDepartmentDto> AccessDepartments { get; } = new();
    public ObservableCollection<DepartmentDto> Departments { get; } = new();

    public List<string> Roles { get; } = new() { "Administrator", "Manager", "Employee" };
    public List<string> LetterTypes { get; } = new() { "Incoming", "Outgoing", "Internal" };
    public List<string> Classifications { get; } = new() { "Public", "Restricted", "Secret" };
    public List<string> Priorities { get; } = new() { "Low", "Medium", "High", "Urgent" };
    public List<string> Statuses { get; } = new() { "New", "InProgress", "Closed" };

    public string BaseUrl
    {
        get => _baseUrl;
        set { _baseUrl = value; OnPropertyChanged(nameof(BaseUrl)); }
    }

    public string UserName
    {
        get => _userName;
        set { _userName = value; OnPropertyChanged(nameof(UserName)); }
    }

    public string LoginStatus
    {
        get => _loginStatus;
        set { _loginStatus = value; OnPropertyChanged(nameof(LoginStatus)); }
    }

    public string LetterSubject
    {
        get => _letterSubject;
        set { _letterSubject = value; OnPropertyChanged(nameof(LetterSubject)); }
    }

    public string SelectedLetterType { get; set; } = "Incoming";
    public string SelectedClassification { get; set; } = "Public";
    public string SelectedPriority { get; set; } = "Medium";
    public string SelectedStatus { get; set; } = "New";
    public DateTime? LetterDueDate { get; set; }
    public DateTime? OutgoingDate { get; set; }

    public string OutgoingChannel
    {
        get => _outgoingChannel;
        set { _outgoingChannel = value; OnPropertyChanged(nameof(OutgoingChannel)); }
    }

    public string OutgoingReference
    {
        get => _outgoingReference;
        set { _outgoingReference = value; OnPropertyChanged(nameof(OutgoingReference)); }
    }

    public string InstitutionName
    {
        get => _institutionName;
        set { _institutionName = value; OnPropertyChanged(nameof(InstitutionName)); }
    }

    public string InstitutionExternalId
    {
        get => _institutionExternalId;
        set { _institutionExternalId = value; OnPropertyChanged(nameof(InstitutionExternalId)); }
    }

    public string InstitutionAddress
    {
        get => _institutionAddress;
        set { _institutionAddress = value; OnPropertyChanged(nameof(InstitutionAddress)); }
    }

    public string InstitutionContact
    {
        get => _institutionContact;
        set { _institutionContact = value; OnPropertyChanged(nameof(InstitutionContact)); }
    }

    public string NewUserName
    {
        get => _newUserName;
        set { _newUserName = value; OnPropertyChanged(nameof(NewUserName)); }
    }

    public string NewUserFullName
    {
        get => _newUserFullName;
        set { _newUserFullName = value; OnPropertyChanged(nameof(NewUserFullName)); }
    }

    public string NewUserRole
    {
        get => _newUserRole;
        set { _newUserRole = value; OnPropertyChanged(nameof(NewUserRole)); }
    }

    public DepartmentDto? SelectedDepartment { get; set; }

    public string DocumentLetterId
    {
        get => _documentLetterId;
        set { _documentLetterId = value; OnPropertyChanged(nameof(DocumentLetterId)); }
    }

    public string DocumentId
    {
        get => _documentId;
        set { _documentId = value; OnPropertyChanged(nameof(DocumentId)); }
    }

    public string ReportOutput
    {
        get => _reportOutput;
        set { _reportOutput = value; OnPropertyChanged(nameof(ReportOutput)); }
    }

    public string NotificationReadId
    {
        get => _notificationReadId;
        set { _notificationReadId = value; OnPropertyChanged(nameof(NotificationReadId)); }
    }

    public string ProtocolYear
    {
        get => _protocolYear;
        set { _protocolYear = value; OnPropertyChanged(nameof(ProtocolYear)); }
    }

    public string LetterActionId
    {
        get => _letterActionId;
        set { _letterActionId = value; OnPropertyChanged(nameof(LetterActionId)); }
    }

    public string AssignUserId
    {
        get => _assignUserId;
        set { _assignUserId = value; OnPropertyChanged(nameof(AssignUserId)); }
    }

    public string ResponseLetterId
    {
        get => _responseLetterId;
        set { _responseLetterId = value; OnPropertyChanged(nameof(ResponseLetterId)); }
    }

    public string ResponseMessage
    {
        get => _responseMessage;
        set { _responseMessage = value; OnPropertyChanged(nameof(ResponseMessage)); }
    }

    public string AccessLetterId
    {
        get => _accessLetterId;
        set { _accessLetterId = value; OnPropertyChanged(nameof(AccessLetterId)); }
    }

    public string AccessUserId
    {
        get => _accessUserId;
        set { _accessUserId = value; OnPropertyChanged(nameof(AccessUserId)); }
    }

    public string AccessDepartmentId
    {
        get => _accessDepartmentId;
        set { _accessDepartmentId = value; OnPropertyChanged(nameof(AccessDepartmentId)); }
    }

    public string DepartmentName
    {
        get => _departmentName;
        set { _departmentName = value; OnPropertyChanged(nameof(DepartmentName)); }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void ApplyAuth()
    {
        if (!string.IsNullOrWhiteSpace(_accessToken))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _client.BaseAddress = new Uri(BaseUrl.TrimEnd('/'));
            var payload = new { userName = UserName, password = PasswordBox.Password };
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);
            if (!response.IsSuccessStatusCode)
            {
                LoginStatus = "Login failed";
                return;
            }
            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
            _accessToken = token?.AccessToken;
            ApplyAuth();
            LoginStatus = "Logged in";
            await RefreshDepartments();
        }
        catch (Exception ex)
        {
            LoginStatus = ex.Message;
        }
    }

    private async void LettersRefresh_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        await RefreshLetters();
    }

    private async void LetterCreate_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        var payload = new
        {
            type = LetterTypes.IndexOf(SelectedLetterType),
            classification = Classifications.IndexOf(SelectedClassification),
            subject = LetterSubject,
            externalInstitutionId = (int?)null,
            createdByUserId = 0,
            priority = Priorities.IndexOf(SelectedPriority),
            dueDate = LetterDueDate,
            outgoingChannel = OutgoingChannel,
            outgoingDate = OutgoingDate,
            outgoingReference = OutgoingReference
        };
        var response = await _client.PostAsJsonAsync("/api/v1/letters", payload);
        if (response.IsSuccessStatusCode)
        {
            LetterSubject = string.Empty;
            OutgoingChannel = string.Empty;
            OutgoingReference = string.Empty;
            OutgoingDate = null;
            await RefreshLetters();
        }
    }

    private async void LetterUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(LetterActionId, out var letterId)) return;
        var payload = new
        {
            classification = Classifications.IndexOf(SelectedClassification),
            subject = LetterSubject,
            externalInstitutionId = (int?)null,
            priority = Priorities.IndexOf(SelectedPriority),
            dueDate = LetterDueDate,
            outgoingChannel = OutgoingChannel,
            outgoingDate = OutgoingDate,
            outgoingReference = OutgoingReference
        };
        await _client.PutAsJsonAsync($"/api/v1/letters/{letterId}", payload);
        await RefreshLetters();
    }

    private async void LetterAssign_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(LetterActionId, out var letterId)) return;
        if (!int.TryParse(AssignUserId, out var userId)) return;
        var payload = new { assignedToUserId = userId, note = (string?)null };
        await _client.PostAsJsonAsync($"/api/v1/letters/{letterId}/assign", payload);
        await RefreshLetters();
    }

    private async void LetterStatus_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(LetterActionId, out var letterId)) return;
        var payload = new { status = Statuses.IndexOf(SelectedStatus) };
        await _client.PostAsJsonAsync($"/api/v1/letters/{letterId}/status", payload);
        await RefreshLetters();
    }

    private async void ResponsesList_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(ResponseLetterId, out var letterId)) return;
        var items = await _client.GetFromJsonAsync<List<ResponseDto>>($"/api/v1/letters/{letterId}/responses");
        SetCollection(Responses, items);
    }

    private async void ResponsesAdd_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(ResponseLetterId, out var letterId)) return;
        var payload = new { message = ResponseMessage };
        var response = await _client.PostAsJsonAsync($"/api/v1/letters/{letterId}/responses", payload);
        if (response.IsSuccessStatusCode)
        {
            ResponseMessage = string.Empty;
            await RefreshResponses(letterId);
        }
    }

    private async void AccessList_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(AccessLetterId, out var letterId)) return;
        var items = await _client.GetFromJsonAsync<List<int>>($"/api/v1/letters/{letterId}/access");
        var users = items?.Select(id => new AccessUserDto { UserId = id }).ToList();
        SetCollection(AccessUsers, users);
        var deptItems = await _client.GetFromJsonAsync<List<int>>($"/api/v1/letters/{letterId}/department-access");
        var departments = deptItems?.Select(id => new AccessDepartmentDto { DepartmentId = id }).ToList();
        SetCollection(AccessDepartments, departments);
    }

    private async void AccessAdd_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(AccessLetterId, out var letterId)) return;
        if (!int.TryParse(AccessUserId, out var userId)) return;
        var payload = new { userId };
        await _client.PostAsJsonAsync($"/api/v1/letters/{letterId}/access", payload);
        await RefreshAccess(letterId);
    }

    private async void AccessRemove_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(AccessLetterId, out var letterId)) return;
        if (!int.TryParse(AccessUserId, out var userId)) return;
        await _client.DeleteAsync($"/api/v1/letters/{letterId}/access/{userId}");
        await RefreshAccess(letterId);
    }

    private async void AccessDepartmentAdd_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(AccessLetterId, out var letterId)) return;
        if (!int.TryParse(AccessDepartmentId, out var departmentId)) return;
        await _client.PostAsync($"/api/v1/letters/{letterId}/department-access/{departmentId}", null);
        await RefreshAccess(letterId);
    }

    private async void AccessDepartmentRemove_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(AccessLetterId, out var letterId)) return;
        if (!int.TryParse(AccessDepartmentId, out var departmentId)) return;
        await _client.DeleteAsync($"/api/v1/letters/{letterId}/department-access/{departmentId}");
        await RefreshAccess(letterId);
    }

    private async void InstitutionsRefresh_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        await RefreshInstitutions();
    }

    private async void InstitutionCreate_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        var payload = new
        {
            name = InstitutionName,
            externalId = InstitutionExternalId,
            address = InstitutionAddress,
            contact = InstitutionContact
        };
        var response = await _client.PostAsJsonAsync("/api/v1/institutions", payload);
        if (response.IsSuccessStatusCode)
        {
            InstitutionName = string.Empty;
            InstitutionExternalId = string.Empty;
            InstitutionAddress = string.Empty;
            InstitutionContact = string.Empty;
            await RefreshInstitutions();
        }
    }

    private async void DepartmentsRefresh_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        await RefreshDepartments();
    }

    private async void DepartmentCreate_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        var payload = new { name = DepartmentName };
        var response = await _client.PostAsJsonAsync("/api/v1/departments", payload);
        if (response.IsSuccessStatusCode)
        {
            DepartmentName = string.Empty;
            await RefreshDepartments();
        }
    }

    private async void UsersRefresh_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        await RefreshUsers();
    }

    private async void UserCreate_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        var payload = new
        {
            userName = NewUserName,
            fullName = NewUserFullName,
            role = NewUserRole,
            password = NewUserPassword.Password,
            departmentId = SelectedDepartment?.Id
        };
        var response = await _client.PostAsJsonAsync("/api/v1/users", payload);
        if (response.IsSuccessStatusCode)
        {
            NewUserName = string.Empty;
            NewUserFullName = string.Empty;
            NewUserPassword.Password = string.Empty;
            await RefreshUsers();
        }
    }

    private async void DocumentList_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(DocumentLetterId, out var letterId)) return;
        await RefreshDocuments(letterId);
    }

    private async void DocumentUpload_Click(object sender, RoutedEventArgs e)
    {
        await UploadDocument("/api/v1/letters/{0}/documents");
    }

    private async void DocumentScan_Click(object sender, RoutedEventArgs e)
    {
        await UploadDocument("/api/v1/letters/{0}/scan");
    }

    private async void DocumentDownload_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(DocumentId, out var docId)) return;
        var response = await _client.GetAsync($"/api/v1/documents/{docId}/download");
        if (!response.IsSuccessStatusCode) return;
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? $"document-{docId}";
        var saveDialog = new SaveFileDialog { FileName = fileName, Filter = "All files|*.*" };
        if (saveDialog.ShowDialog() == true)
        {
            await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
        }
    }

    private async void DocumentDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(DocumentId, out var docId)) return;
        await _client.DeleteAsync($"/api/v1/documents/{docId}");
    }

    private async Task UploadDocument(string urlFormat)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(DocumentLetterId, out var letterId)) return;
        var dialog = new OpenFileDialog();
        if (dialog.ShowDialog() != true) return;
        await using var stream = File.OpenRead(dialog.FileName);
        using var form = new MultipartFormDataContent();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(content, "file", Path.GetFileName(dialog.FileName));
        var response = await _client.PostAsync(string.Format(urlFormat, letterId), form);
        if (response.IsSuccessStatusCode)
        {
            await RefreshDocuments(letterId);
        }
    }

    private async void ReportSummary_Click(object sender, RoutedEventArgs e)
    {
        await LoadReport("/api/v1/reports/summary");
    }

    private async void ReportOverdue_Click(object sender, RoutedEventArgs e)
    {
        await LoadReport("/api/v1/reports/overdue");
    }

    private async void ReportByUser_Click(object sender, RoutedEventArgs e)
    {
        await LoadReport("/api/v1/reports/by-user");
    }

    private async void ReportTracking_Click(object sender, RoutedEventArgs e)
    {
        await LoadReport("/api/v1/reports/tracking");
    }

    private async void ReportByPriority_Click(object sender, RoutedEventArgs e)
    {
        await LoadReport("/api/v1/reports/by-priority");
    }

    private async void ReportByStatus_Click(object sender, RoutedEventArgs e)
    {
        await LoadReport("/api/v1/reports/by-status");
    }

    private async void ReportByDepartment_Click(object sender, RoutedEventArgs e)
    {
        await LoadReport("/api/v1/reports/by-department");
    }

    private async Task LoadReport(string path)
    {
        if (!EnsureAuth()) return;
        var response = await _client.GetAsync(path);
        ReportOutput = await response.Content.ReadAsStringAsync();
    }

    private async void NotificationsRefresh_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        await RefreshNotifications();
    }

    private async void NotificationMarkRead_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(NotificationReadId, out var id)) return;
        var response = await _client.PostAsync($"/api/v1/notifications/{id}/read", null);
        if (response.IsSuccessStatusCode)
        {
            await RefreshNotifications();
        }
    }

    private async void ProtocolOpen_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(ProtocolYear, out var year)) return;
        await _client.PostAsJsonAsync("/api/v1/protocol-books/open", year);
    }

    private async void ProtocolClose_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(ProtocolYear, out var year)) return;
        await _client.PostAsJsonAsync("/api/v1/protocol-books/close", year);
    }

    private async void ProtocolDownload_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(ProtocolYear, out var year)) return;
        var response = await _client.GetAsync($"/api/v1/protocol-books/{year}/print");
        if (!response.IsSuccessStatusCode) return;
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var saveDialog = new SaveFileDialog { FileName = $"protocol-book-{year}.csv", Filter = "CSV|*.csv" };
        if (saveDialog.ShowDialog() == true)
        {
            await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
        }
    }

    private async void ProtocolPrint_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureAuth()) return;
        if (!int.TryParse(ProtocolYear, out var year)) return;
        var items = await _client.GetFromJsonAsync<List<LetterDto>>($"/api/v1/protocol-books/{year}/items");
        if (items == null) return;
        var doc = BuildProtocolBookDocument(year, items);
        var dialog = new PrintDialog();
        if (dialog.ShowDialog() == true)
        {
            dialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, $"Protocol Book {year}");
        }
    }

    private FlowDocument BuildProtocolBookDocument(int year, List<LetterDto> items)
    {
        var doc = new FlowDocument { PagePadding = new Thickness(40) };
        doc.Blocks.Add(new Paragraph(new Run($"Protocol Book {year}")) { FontSize = 16, FontWeight = FontWeights.Bold });
        var table = new Table();
        table.Columns.Add(new TableColumn { Width = new GridLength(120) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });
        table.Columns.Add(new TableColumn { Width = new GridLength(110) });
        table.Columns.Add(new TableColumn { Width = new GridLength(240) });
        table.Columns.Add(new TableColumn { Width = new GridLength(90) });
        var header = new TableRow();
        header.Cells.Add(new TableCell(new Paragraph(new Run("Protocol"))));
        header.Cells.Add(new TableCell(new Paragraph(new Run("Type"))));
        header.Cells.Add(new TableCell(new Paragraph(new Run("Class"))));
        header.Cells.Add(new TableCell(new Paragraph(new Run("Subject"))));
        header.Cells.Add(new TableCell(new Paragraph(new Run("Status"))));
        var headerGroup = new TableRowGroup();
        headerGroup.Rows.Add(header);
        table.RowGroups.Add(headerGroup);
        var group = new TableRowGroup();
        foreach (var item in items)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.ProtocolNumber))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Type))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Classification))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Subject))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Status))));
            group.Rows.Add(row);
        }
        table.RowGroups.Add(group);
        doc.Blocks.Add(table);
        return doc;
    }

    private bool EnsureAuth()
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
        {
            LoginStatus = "Login first";
            return false;
        }
        return true;
    }

    private static void SetCollection<T>(ObservableCollection<T> collection, List<T>? items)
    {
        collection.Clear();
        if (items == null) return;
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    private async Task RefreshLetters()
    {
        var items = await _client.GetFromJsonAsync<List<LetterDto>>("/api/v1/letters");
        SetCollection(Letters, items);
    }

    private async Task RefreshInstitutions()
    {
        var items = await _client.GetFromJsonAsync<List<InstitutionDto>>("/api/v1/institutions");
        SetCollection(Institutions, items);
    }

    private async Task RefreshUsers()
    {
        var items = await _client.GetFromJsonAsync<List<UserDto>>("/api/v1/users");
        SetCollection(Users, items);
    }

    private async Task RefreshDocuments(int letterId)
    {
        var items = await _client.GetFromJsonAsync<List<DocumentDto>>($"/api/v1/letters/{letterId}/documents");
        SetCollection(Documents, items);
    }

    private async Task RefreshNotifications()
    {
        var items = await _client.GetFromJsonAsync<List<NotificationDto>>("/api/v1/notifications?unreadOnly=false");
        SetCollection(Notifications, items);
    }

    private async Task RefreshResponses(int letterId)
    {
        var items = await _client.GetFromJsonAsync<List<ResponseDto>>($"/api/v1/letters/{letterId}/responses");
        SetCollection(Responses, items);
    }

    private async Task RefreshAccess(int letterId)
    {
        var items = await _client.GetFromJsonAsync<List<int>>($"/api/v1/letters/{letterId}/access");
        var users = items?.Select(id => new AccessUserDto { UserId = id }).ToList();
        SetCollection(AccessUsers, users);
        var deptItems = await _client.GetFromJsonAsync<List<int>>($"/api/v1/letters/{letterId}/department-access");
        var departments = deptItems?.Select(id => new AccessDepartmentDto { DepartmentId = id }).ToList();
        SetCollection(AccessDepartments, departments);
    }

    private async Task RefreshDepartments()
    {
        var items = await _client.GetFromJsonAsync<List<DepartmentDto>>("/api/v1/departments");
        SetCollection(Departments, items);
    }
}
