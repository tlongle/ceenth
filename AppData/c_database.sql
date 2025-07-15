-- Este script cria a table "Presets" que vai conter os nossos sons por defeito para o sintetizador
-- O sintetizador vem com sons por defeito para o utilizador testar o programa
-- Podendo depois criar sons novos, remover, dar update, ou até mesmo delete
-- Os sons que veem com o programa nao podem ser deletados

IF OBJECT_ID('dbo.Presets', 'U') IS NOT NULL
DROP TABLE dbo.Presets;
GO

-- Create the Presets table
CREATE TABLE dbo.Presets
(
    IdPreset INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    IsUserPreset BIT NOT NULL,
    Waveform INT NOT NULL,
    AttackSeconds REAL NOT NULL,
    ReleaseSeconds REAL NOT NULL,
    FilterCutoff REAL NOT NULL,
    FilterQ REAL NOT NULL,
    VibratoRate REAL NOT NULL,
    VibratoDepth REAL NOT NULL,
    TremoloRate REAL NOT NULL,
    TremoloDepth REAL NOT NULL
);
GO

-- Seed the table with the default factory presets.
-- We use SET IDENTITY_INSERT ON to manually set the IDs for these presets.
SET IDENTITY_INSERT dbo.Presets ON;

INSERT INTO dbo.Presets (IdPreset, Name, IsUserPreset, Waveform, AttackSeconds, ReleaseSeconds, FilterCutoff, FilterQ, VibratoRate, VibratoDepth, TremoloRate, TremoloDepth)
VALUES
(1, 'Default Sine', 0, 0, 0.01, 0.3, 1.0, 1.0, 5, 0, 5, 0),
(2, 'Simple Bass', 0, 1, 0.02, 0.4, 0.3, 2.0, 0, 0, 0, 0),
(3, 'Wobble Pad', 0, 2, 0.8, 1.5, 0.6, 4.0, 6, 8, 0, 0),
(4, 'Laser Sweep', 0, 1, 0.01, 0.01, 0.98, 7.3, 5.0, 20.0, 5.0, 0.5);

SET IDENTITY_INSERT dbo.Presets OFF;
GO
