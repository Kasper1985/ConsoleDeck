<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <!-- Identity template: Copy everything by default -->
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <!-- Template for the root Wix element -->
    <xsl:template match="/Wix">
        <Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
            <!-- Copy attributes of Wix -->
            <xsl:copy-of select="@*"/>
            <!-- Insert the Product element here -->
            <Product Id="*" 
                     Name="ConsoleDeck Service" 
                     Language="1033" 
                     Version="0.9.0" 
                     Manufacturer="Kasper Inc." 
                     UpgradeCode="{c1f8372d-ab90-45cb-90d7-dc75fd0e0897}">
                <Package InstallerVersion="200" 
                         Compressed="yes" 
                         InstallScope="perMachine" 
                         Description="Installer for ConsoleDeck Service" />
                <!-- Prevent downgrades -->
                <MajorUpgrade DowngradeErrorMessage="A newer version of ConsoleDeck Service is already installed." />
                <!-- UI: Use a basic UI sequence -->
                <UIRef Id="WixUI_Minimal" />
                <!-- Media: Single CAB file for compression -->
                <MediaTemplate EmbedCab="yes" />
                <!-- Feature: What gets installed -->
                <Feature Id="ProductFeature" 
                         Title="ConsoleDeck Service" 
                         Description="Installs the ConsoleDeck Service application" 
                         Level="1" 
                         ConfigurableDirectory="INSTALLFOLDER">
                    <ComponentGroupRef Id="ProductComponents" />
                </Feature>
            </Product>
            <!-- Directory structure -->
            <Fragment>
                <Directory Id="TARGETDIR" Name="SourceDir">
                <Directory Id="ProgramFilesFolder">
                    <Directory Id="INSTALLFOLDER" Name="ConsoleDeckService" />
                </Directory>
                <Directory Id="ProgramMenuFolder">
                    <Directory Id="ApplicationProgramsFolder" Name="ConsoleDeckService" />
                </Directory>
                </Directory>
            </Fragment>
            <!-- Apply templates to the rest of the content (Fragments) -->
            <xsl:apply-templates select="node()"/>
        </Wix>
    </xsl:template>
</xsl:stylesheet>