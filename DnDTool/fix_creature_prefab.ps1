$prefabPath = "d:\Temp\Dnd\DnDTool\DnDTool\DnDTool\DnDTool\Assets\AssetRaw\UI\ChapterCreatureEntryPopupUI.prefab"
$backupPath = $prefabPath + ".bak"
Copy-Item $prefabPath $backupPath -Force
Write-Host "Backup: $backupPath"

$lines = [System.IO.File]::ReadAllLines($prefabPath)
Write-Host "Lines: $($lines.Count)"

$templateLabelStart=-1; $templateEndExcl=-1; $insertionPoint=-1; $mcompsLine=-1; $contentChildrenLine=-1
for ($i=0;$i -lt $lines.Count;$i++) {
    $l=$lines[$i]
    if ($l -eq "--- !u!1 &1500") { $templateLabelStart=$i }
    if ($l -eq "--- !u!1 &1700") { $templateEndExcl=$i }
    if ($l -eq "--- !u!1 &5600") { $insertionPoint=$i }
    if ($l -eq "  m_components:")  { $mcompsLine=$i }
    if ($l -eq "  - {fileID: 1501}") { $contentChildrenLine=$i }
}
Write-Host "tLS=$templateLabelStart tEnd=$templateEndExcl ins=$insertionPoint mc=$mcompsLine cch=$contentChildrenLine"
if ($templateLabelStart -lt 0 -or $templateEndExcl -lt 0 -or $insertionPoint -lt 0 -or $mcompsLine -lt 0 -or $contentChildrenLine -lt 0) { Write-Error "KEY LINE NOT FOUND"; exit 1 }

$tmplText = ($lines[$templateLabelStart..($templateEndExcl-1)]) -join "`n"

# Each field: base, fieldName, labelName, labelEsc, labelY, lineType, inputH
$fields = @(
  @{b=6000;fn="m_tmpInputCreatureNameEn";            ln="m_tmpNameEnLabel";              le="\u82F1\u6587\u540D\u79F0";lY=-870; lt=0;ih=34},
  @{b=6100;fn="m_tmpInputCreatureExperiencePoints";  ln="m_tmpExperiencePointsLabel";    le="\u7ECF\u9A8C\u503C";       lY=-938; lt=0;ih=34},
  @{b=6200;fn="m_tmpInputCreatureDex";               ln="m_tmpDexLabel";                 le="\u654F\u6377DEX";          lY=-1006;lt=0;ih=34},
  @{b=6300;fn="m_tmpInputCreatureCon";               ln="m_tmpConLabel";                 le="\u4F53\u8D28CON";          lY=-1074;lt=0;ih=34},
  @{b=6400;fn="m_tmpInputCreatureInt";               ln="m_tmpIntLabel";                 le="\u667A\u529BINT";          lY=-1142;lt=0;ih=34},
  @{b=6500;fn="m_tmpInputCreatureWis";               ln="m_tmpWisLabel";                 le="\u611F\u77E5WIS";          lY=-1210;lt=0;ih=34},
  @{b=6600;fn="m_tmpInputCreatureCha";               ln="m_tmpChaLabel";                 le="\u9B45\u529BCHA";          lY=-1278;lt=0;ih=34},
  @{b=6700;fn="m_tmpInputCreatureSavingThrows";      ln="m_tmpSavingThrowsLabel";        le="\u8C41\u514D\u68C0\u5B9A";lY=-1346;lt=0;ih=34},
  @{b=6800;fn="m_tmpInputCreatureSkills";            ln="m_tmpSkillsLabel";              le="\u6280\u80FD\u52A0\u503C"; lY=-1414;lt=0;ih=34},
  @{b=6900;fn="m_tmpInputCreatureDamageImmunities";  ln="m_tmpDamageImmunitiesLabel";    le="\u4F24\u5BB3\u514D\u75AB";lY=-1482;lt=0;ih=34},
  @{b=7000;fn="m_tmpInputCreatureConditionImmunities";ln="m_tmpConditionImmunitiesLabel";le="\u72B6\u6001\u514D\u75AB";lY=-1550;lt=0;ih=34},
  @{b=7100;fn="m_tmpInputCreatureReactions";         ln="m_tmpReactionsLabel";           le="\u53CD\u5E94";             lY=-1618;lt=2;ih=58},
  @{b=7200;fn="m_tmpInputCreatureLegendaryActions";  ln="m_tmpLegendaryActionsLabel";    le="\u4F20\u5947\u52A8\u4F5C";lY=-1710;lt=2;ih=58}
)

$newYaml=[System.Collections.Generic.List[string]]::new()

foreach ($f in $fields) {
  $b=$f.b
  $lGO=$b+30;$lRT=$b+31;$lCR=$b+32;$lTMP=$b+33
  $iGO=$b;   $iRT=$b+1; $iCR=$b+2; $iImg=$b+3; $iTMPIF=$b+4
  $pGO=$b+10;$pRT=$b+11;$pCR=$b+12;$pTMP=$b+13
  $tGO=$b+20;$tRT=$b+21;$tCR=$b+22;$tTMP=$b+23

  $t=$tmplText
  $t=$t -replace "704344806803944032",  $lTMP
  $t=$t -replace "3896897294230710720", $iTMPIF
  $t=$t -replace "9159947957406032403", $pTMP
  $t=$t -replace "5817826280247259795", $tTMP
  $t=$t -replace "(?<!\d)1500(?!\d)", $lGO
  $t=$t -replace "(?<!\d)1501(?!\d)", $lRT
  $t=$t -replace "(?<!\d)1502(?!\d)", $lCR
  $t=$t -replace "(?<!\d)1600(?!\d)", $iGO
  $t=$t -replace "(?<!\d)1601(?!\d)", $iRT
  $t=$t -replace "(?<!\d)1602(?!\d)", $iCR
  $t=$t -replace "(?<!\d)1603(?!\d)", $iImg
  $t=$t -replace "(?<!\d)1610(?!\d)", $pGO
  $t=$t -replace "(?<!\d)1611(?!\d)", $pRT
  $t=$t -replace "(?<!\d)1612(?!\d)", $pCR
  $t=$t -replace "(?<!\d)1620(?!\d)", $tGO
  $t=$t -replace "(?<!\d)1621(?!\d)", $tRT
  $t=$t -replace "(?<!\d)1622(?!\d)", $tCR

  $t=$t -replace "  m_Name: m_tmpNameLabel",          "  m_Name: $($f.ln)"
  $t=$t -replace "  m_Name: m_tmpInputCreatureName",  "  m_Name: $($f.fn)"
  $t=$t -replace '\\u751F\\u7269\\u540D\\u79F0',      $f.le
  $t=$t -replace 'm_AnchoredPosition: \{x: 9\.2999, y: 0\}',     "m_AnchoredPosition: {x: 9.299957, y: $($f.lY)}"
  $t=$t -replace 'm_AnchoredPosition: \{x: 9\.299957, y: -24\}', "m_AnchoredPosition: {x: 9.299957, y: $($f.lY - 24)}"
  if ($f.lt -eq 2) {
    $t=$t -replace 'm_SizeDelta: \{x: 344\.5414, y: 34\}', "m_SizeDelta: {x: 721.5101, y: $($f.ih)}"
    $t=$t -replace '  m_LineType: 0', "  m_LineType: 2"
  }
  $newYaml.AddRange(($t -split "`n"))
  Write-Host "  + $($f.fn)"
}
Write-Host "New YAML lines: $($newYaml.Count)"

$newComps = @(
  "  m_components:",
  "  - {fileID: 1304}","  - {fileID: 2404}","  - {fileID: 5304}",
  "  - {fileID: 5603}","  - {fileID: 5704}","  - {fileID: 5803}",
  "  - {fileID: 3896897294230710720}",
  "  - {fileID: 6004}",
  "  - {fileID: 892234324161868822}",
  "  - {fileID: 6284232223408213472}",
  "  - {fileID: 6459136875375728515}",
  "  - {fileID: 7849509450769382037}",
  "  - {fileID: 6104}",
  "  - {fileID: 5180769901779233174}",
  "  - {fileID: 4733851912287492426}",
  "  - {fileID: 2287296744311417362}",
  "  - {fileID: 1245589738655963474}",
  "  - {fileID: 6204}","  - {fileID: 6304}","  - {fileID: 6404}",
  "  - {fileID: 6504}","  - {fileID: 6604}",
  "  - {fileID: 6704}","  - {fileID: 6804}",
  "  - {fileID: 666882539057752068}",
  "  - {fileID: 1119637889168062665}",
  "  - {fileID: 1523703713018127394}",
  "  - {fileID: 6904}","  - {fileID: 7004}",
  "  - {fileID: 4677915984755088761}",
  "  - {fileID: 358408439401705766}",
  "  - {fileID: 8641298465343803182}",
  "  - {fileID: 7104}","  - {fileID: 7204}",
  "  - {fileID: 2680867090532706110}"
)

$mcEnd=$mcompsLine+1
while ($mcEnd -lt $lines.Count -and $lines[$mcEnd] -match "^\s+- \{fileID:") { $mcEnd++ }
Write-Host "m_components: $mcompsLine to $($mcEnd-1)"

$ccEnd=$contentChildrenLine
while ($ccEnd -lt $lines.Count -and $lines[$ccEnd] -match "^\s+- \{fileID:") { $ccEnd++ }
Write-Host "Content children: $contentChildrenLine to $($ccEnd-1)"

$newChildren=[System.Collections.Generic.List[string]]::new()
$newChildren.AddRange($lines[$contentChildrenLine..($ccEnd-1)])
foreach ($f in $fields) {
  $newChildren.Add("  - {fileID: $($f.b+31)}")
  $newChildren.Add("  - {fileID: $($f.b+1)}")
}

$sdLine=-1
for ($i=$ccEnd;$i -lt [Math]::Min($ccEnd+30,$lines.Count);$i++) {
  if ($lines[$i] -match "m_SizeDelta:.*y: 876") { $sdLine=$i; break }
}
Write-Host "SizeDelta line: $sdLine"

$out=[System.Collections.Generic.List[string]]::new()
$cur=0
$out.AddRange($lines[$cur..($mcompsLine-1)]); $out.AddRange($newComps); $cur=$mcEnd
$out.AddRange($lines[$cur..($contentChildrenLine-1)]); $out.AddRange($newChildren); $cur=$ccEnd
if ($sdLine -gt $cur) {
  $out.AddRange($lines[$cur..($sdLine-1)])
  $out.Add("  m_SizeDelta: {x: 0, y: 1820}")
  $cur=$sdLine+1
}
if ($cur -lt $insertionPoint) { $out.AddRange($lines[$cur..($insertionPoint-1)]) }
$out.AddRange($newYaml)
$out.AddRange($lines[$insertionPoint..($lines.Count-1)])

Write-Host "Output lines: $($out.Count) (was $($lines.Count), delta $($out.Count - $lines.Count))"
[System.IO.File]::WriteAllLines($prefabPath, $out, [System.Text.Encoding]::UTF8)
Write-Host "Done."
