#version 4
if (KahuRestrictionsLoading ne "1") {
    KahuRestrictionsLoading = "1";
    loadVariables("KahuRestrictions.txt", "../correct", "GET");
}
stop();
