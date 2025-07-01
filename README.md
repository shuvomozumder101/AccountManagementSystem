# AccountManagementSystem

USE AccountManagementDB;
GO

CREATE TABLE ChartOfAccounts (
    AccountId INT PRIMARY KEY IDENTITY(1,1),
    AccountCode NVARCHAR(50) NOT NULL UNIQUE,
    AccountName NVARCHAR(255) NOT NULL,
    AccountType NVARCHAR(50) NOT NULL,
    ParentAccountId INT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_ChartOfAccounts_ParentAccountId FOREIGN KEY (ParentAccountId) REFERENCES ChartOfAccounts(AccountId)
);
GO

CREATE TABLE Vouchers (
    VoucherId INT PRIMARY KEY IDENTITY(1,1),
    VoucherType NVARCHAR(50) NOT NULL,
    VoucherDate DATE NOT NULL,
    ReferenceNo NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(MAX) NULL,
	TotalDebit DECIMAL(20,2) DEFAULT 0,
    TotalCredit DECIMAL(20,2) DEFAULT 0,
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE VoucherDetails (
    VoucherDetailId INT PRIMARY KEY IDENTITY(1,1),
    VoucherId INT NOT NULL,
    AccountId INT NOT NULL,
    DebitAmount DECIMAL(18, 2) DEFAULT 0,
    CreditAmount DECIMAL(18, 2) DEFAULT 0,
    Narration NVARCHAR(MAX) NULL,
    CONSTRAINT FK_VoucherDetails_VoucherId FOREIGN KEY (VoucherId) REFERENCES Vouchers(VoucherId),
    CONSTRAINT FK_VoucherDetails_AccountId FOREIGN KEY (AccountId) REFERENCES ChartOfAccounts(AccountId)
);
GO

CREATE PROCEDURE sp_ManageChartOfAccounts
    @Action VARCHAR(50),
    @AccountId INT = NULL,
    @AccountCode NVARCHAR(50) = NULL,
    @AccountName NVARCHAR(255) = NULL,
    @AccountType NVARCHAR(50) = NULL,
    @ParentAccountId INT = NULL,
    @IsActive BIT = NULL,
    @OutputAccountId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @OutputAccountId = 0;

    IF @Action = 'Insert'
    BEGIN
        INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountId, IsActive, CreatedDate)
        VALUES (@AccountCode, @AccountName, @AccountType, @ParentAccountId, @IsActive, GETDATE());

        SET @OutputAccountId = SCOPE_IDENTITY();
    END
    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE ChartOfAccounts
        SET
            AccountCode = ISNULL(@AccountCode, AccountCode),
            AccountName = ISNULL(@AccountName, AccountName),
            AccountType = ISNULL(@AccountType, AccountType),
            ParentAccountId = ISNULL(@ParentAccountId, ParentAccountId),
            IsActive = ISNULL(@IsActive, IsActive)
        WHERE AccountId = @AccountId;
    END
    ELSE IF @Action = 'Delete'
    BEGIN
        IF EXISTS (SELECT 1 FROM ChartOfAccounts WHERE ParentAccountId = @AccountId)
        BEGIN
            RAISERROR('Cannot delete account: It has child accounts.', 16, 1);
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM VoucherDetails WHERE AccountId = @AccountId)
        BEGIN
            RAISERROR('Cannot delete account: It has associated voucher details.', 16, 1);
            RETURN;
        END

        DELETE FROM ChartOfAccounts
        WHERE AccountId = @AccountId;
    END
    ELSE IF @Action = 'Select'
    BEGIN
        SELECT
            AccountId,
            AccountCode,
            AccountName,
            AccountType,
            ParentAccountId,
            IsActive,
            CreatedDate
        FROM ChartOfAccounts
        WHERE @AccountId IS NULL OR AccountId = @AccountId
        ORDER BY AccountCode;
    END
    ELSE IF @Action = 'SelectParents'
    BEGIN
        SELECT
            AccountId,
            AccountCode,
            AccountName,
            AccountType,
            ParentAccountId,
            IsActive,
            CreatedDate
        FROM ChartOfAccounts
        WHERE IsActive = 1 
        ORDER BY AccountCode;
    END
    ELSE
    BEGIN
        RAISERROR('Invalid action specified. Use ''Insert'', ''Update'', ''Delete'', ''Select'', or ''SelectParents''.', 16, 1);
    END
END;
GO

IF OBJECT_ID('sp_GetAccountsForDropdown', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetAccountsForDropdown;
GO

CREATE PROCEDURE sp_GetAccountsForDropdown
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AccountId,
        AccountCode,
        AccountName,
        AccountType
    FROM ChartOfAccounts
    WHERE IsActive = 1
    ORDER BY AccountCode;
END;
GO


CREATE PROCEDURE sp_SaveVoucher
    @VoucherType NVARCHAR(50),
    @VoucherDate DATE,
    @ReferenceNo NVARCHAR(100),
    @Description NVARCHAR(MAX),
    @CreatedBy NVARCHAR(255),
	@CreatedDate DATE,
    @VoucherDetailsXml XML,
    @OutputVoucherId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; 

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Vouchers (VoucherType, VoucherDate, ReferenceNo, Description, CreatedBy, CreatedDate)
        VALUES (@VoucherType, @VoucherDate, @ReferenceNo, @Description, @CreatedBy, GETDATE());

        SET @OutputVoucherId = SCOPE_IDENTITY();

        INSERT INTO VoucherDetails (VoucherId, AccountId, DebitAmount, CreditAmount, Narration)
        SELECT
            @OutputVoucherId,
            T.Item.value('AccountId[1]', 'INT'),
            T.Item.value('DebitAmount[1]', 'DECIMAL(18,2)'),
            T.Item.value('CreditAmount[1]', 'DECIMAL(18,2)'),
            T.Item.value('Narration[1]', 'NVARCHAR(MAX)')
        FROM @VoucherDetailsXml.nodes('/VoucherDetails/Detail') AS T(Item);

        DECLARE @TotalDebit DECIMAL(18, 2);
        DECLARE @TotalCredit DECIMAL(18, 2);

        SELECT @TotalDebit = SUM(DebitAmount), @TotalCredit = SUM(CreditAmount)
        FROM VoucherDetails
        WHERE VoucherId = @OutputVoucherId;

        IF @TotalDebit <> @TotalCredit
        BEGIN
            RAISERROR('Total Debit must equal Total Credit for the voucher.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN -1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000);
        DECLARE @ErrorSeverity INT;
        DECLARE @ErrorState INT;

        SELECT
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN -1;
    END CATCH
END;
GO

CREATE PROCEDURE sp_GetUserRoles
    @UserId NVARCHAR(450)
AS
BEGIN
    SELECT r.Name AS RoleName
    FROM AspNetUserRoles ur
    JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @UserId;
END;

CREATE PROCEDURE sp_AssignUserRole
    @UserId NVARCHAR(450),
    @RoleName NVARCHAR(256)
AS
BEGIN
    DECLARE @RoleId NVARCHAR(450);
    SELECT @RoleId = Id FROM AspNetRoles WHERE Name = @RoleName;

    IF @RoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);
    END
END;

CREATE PROCEDURE sp_GetVouchers
    @VoucherId INT = NULL,
    @VoucherType NVARCHAR(50) = NULL,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.VoucherId,
        V.VoucherType,
        V.VoucherDate,
        V.ReferenceNo,
        V.Description,
        V.CreatedBy,
        V.CreatedDate,
        VD.VoucherDetailId,
        VD.AccountId,
        CA.AccountCode,
        CA.AccountName,
        VD.DebitAmount,
        VD.CreditAmount,
        VD.Narration
    FROM Vouchers AS V
    INNER JOIN VoucherDetails AS VD ON V.VoucherId = VD.VoucherId
    INNER JOIN ChartOfAccounts AS CA ON VD.AccountId = CA.AccountId
    WHERE
        (@VoucherId IS NULL OR V.VoucherId = @VoucherId)
        AND (@VoucherType IS NULL OR V.VoucherType = @VoucherType)
        AND (@StartDate IS NULL OR V.VoucherDate >= @StartDate)
        AND (@EndDate IS NULL OR V.VoucherDate <= @EndDate)
    ORDER BY V.VoucherDate DESC, V.VoucherId, VD.VoucherDetailId;
END;
GO
