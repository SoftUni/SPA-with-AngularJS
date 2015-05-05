app.controller("FirstDemoController", function ($scope) {
    var people = [
        {
            name: "Angel",
            hometown: "Plovdiv"
        },
        {
            name: "Mihail",
            hometown: "Varna"
        },
        {
            name: "Georgi",
            hometown: "Sofia"
        },
        {
            name: "Cvetomir",
            hometown: "Kaspichan"
        }
    ];

    $scope.people = people;

    $scope.firstMessage = "First Message";
});