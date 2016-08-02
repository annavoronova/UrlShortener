var UrlShortenerApp = angular.module('UrlShortenerApp', []);

UrlShortenerApp.controller('UrlShortenerController', function ($scope, UrlShortenerService) {
    function getUrls() {
        UrlShortenerService.getUrlList()
            .success(function (urls) {
                $scope.urls = urls;
                $scope.hasNoData = urls.length == 0;
                console.log($scope.urls);
            })
            .error(function (error) {
                $scope.status = 'Unable to load customer data: ' + error.message;
                console.log($scope.status);
            });
    }

    getUrls();
});

UrlShortenerApp.factory('UrlShortenerService', ['$http', function ($http) {

    var UrlShortenerService = {};
    UrlShortenerService.getUrlList = function () {
        return $http.get('ListUrls');
    };
    return UrlShortenerService;

}]);

UrlShortenerApp.filter("jsDate", function () {
    return function (x) {
        return new Date(parseInt(x.substr(6)));
    };
});