var store = new Vuex.Store({
    state: {
        trip: {},
        timelineCreating: false
    },
    actions: {
        /** Trip info modifiers */
        getTripInfo: function (context) {
            axios.get("/Trip/TripInfo/" + TRIP_ID).then(function(response) {
                if (response.data !== null) {
                    response.data.viewType = 'overview'
                    if (typeof response.data.starsAvg !== 'undefined'){
                        response.data.starsAvg = Math.round(response.data.starsAvg);
                    }
                    context.commit("setTrip", response.data)
                }
            }).catch(function(info){
                console.log(info)
            });
        },
        tripStarred: function (context, value) {
            if (context.state.trip.starredValue !== null) return;

            axios.post("/Trip/AddStarRecord/" + TRIP_ID, {
                value: value
            }).then(function (response) {
                console.log(response)
            }).catch(function(info){
                console.log(info)
            })

            context.commit("setTripStars", value);
        },
        saveChanges: function (context) {
            context.dispatch('saveSortingState')
            context.commit('setTripDescription', $(descriptionInstance).summernote('code'))

            var data = {
                isPublic: context.state.trip.isPublic,
                title: context.state.trip.title,
                description: context.state.trip.description
            }

            axios.post("/Trip/SaveTrip/" + TRIP_ID, data)
            .then(function (response) {
                console.log(response)

                galleryDropzoneInstance.processQueue();
            }).catch(function(info) {
                console.log(info)
            });
        },
        deleteTrip: function (context) {
            axios.post("/Trip/DeleteTrip/" + TRIP_ID)
            .then(function (response) {
                window.location.href="/Trip/MyTrips"
            }).catch(function(info) {
                console.log(info)
            });
        },
        setPhotoBasic: function (context, photo) {
            axios.post("/Trip/SetBasicPhoto/" + TRIP_ID, photo)
            .then(function (response) {
                console.log(response)
                context.commit('setPhotoBasic', photo);
            }).catch(function(info) {
                console.log(info)
            });
        },
        deletePhoto: function (context, photo) {
            axios.post("/Trip/DeletePhoto/" + TRIP_ID, photo)
            .then(function (response) {
                console.log(response)
                context.commit('deletePhoto', photo);
            }).catch(function(info) {
                console.log(info)
            });
        },
        saveSortingState: function (context) {
            var sortingData = [];
            var locations = $("#timeline > .timeline-location");

            for (var i = 0; i < locations.length; i++) {
                var locationData = {
                    locationId: $(locations[i]).attr("data-id") || null,
                    position: i+1,
                    events: []
                }
                var events = $(locations[i]).find(".timeline-event");
                
                for (var j = 0; j < events.length; j++) {
                    locationData.events.push({
                        eventId: $(events[j]).attr("data-id") || null,
                        position: j+1
                    })
                }

                sortingData.push(locationData)
            }

            axios.post("/Trip/SaveSortingState/" + TRIP_ID, {
                sorting: sortingData
            })
            .then(function (response) {
                console.log(response)
            }).catch(function(info) {
                console.log(info)
            });
        },

        /** Adding placeholders for new locations and events */
        addDefaultLocation: function (context) {
            axios.get("/Trip/DefaultLocation").then(function(response) {
                if (response.data !== null) {
                    context.commit("addDefaultLocation", response.data)
                }
            }).catch(function(info){
                console.log(info)
            });
        },
        addDefaultEvent: function (context, type) {
            axios.get("/Trip/DefaultEvent?type=" + type).then(function(response) {
                if (response.data !== null) {
                    context.commit("addDefaultEvent", response.data)
                }
            }).catch(function(info){
                console.log(info)
            });
        },

        /** Location and event create/update/delete */
        createLocation: function (context, data) {
            context.commit('setTimelineCreating', false)
            data.mark = "created"
            axios.post("/Trip/CreateLocation/" + TRIP_ID, data)
            .then(function (response) {
                if (response.data !== null && response.data.id !== null){ 
                    data.id = response.data.id
                }
            }).catch(function(info) {
                console.log(info)
            });
        },
        saveLocation: function (context, data) {
            axios.post("/Trip/SaveLocation/" + TRIP_ID, data)
            .then(function (response) {
                if (typeof response.data.dateUpdate !== 'undefined') {
                    data.dateUpdate = response.data.dateUpdate
                }
            }).catch(function(info) {
                console.log(info)
            });
        },
        deleteLocation: function (context, data) {
            if (typeof data.id !== 'undefined' && data.id !== null) {
                var id = data.id
                axios.post("/Trip/DeleteLocation/" + TRIP_ID, {
                    locationId: id
                }).then(function(response){
                    console.log(response)
                }).catch(function(info) {
                    console.log(info)
                })
            }

            if (typeof data.mark !== 'undefined' && data.mark == 'new') {
                context.commit('setTimelineCreating', false)
            }
            context.commit("removeLocation", data)
        },
        createEvent: function (context, data) {
            context.commit('setTimelineCreating', false)
            data.mark = "created"
            axios.post("/Trip/CreateEvent/" + TRIP_ID, data)
            .then(function (response) {
                if (response.data !== null && response.data.id !== null){ 
                    data.id = response.data.id
                    data.locationId = response.data.locationId
                }
            }).catch(function(info) {
                console.log(info)
            });
        },
        saveEvent: function (context, data) {
            axios.post("/Trip/SaveEvent/" + TRIP_ID, data)
            .then(function (response) {
                if (typeof response.data.dateUpdate !== 'undefined') {
                    data.dateUpdate = response.data.dateUpdate
                }
            }).catch(function(info) {
                console.log(info)
            });
        },
        deleteEvent: function (context, data) {
            if (typeof data.id !== 'undefined' && data.id !== null) {
                var id = data.id
                axios.post("/Trip/DeleteEvent/" + TRIP_ID, {
                    eventId: id
                }).then(function(response){
                    console.log(response)
                }).catch(function(info) {
                    console.log(info)
                })
            }

            if (typeof data.mark !== 'undefined' && data.mark == 'new') {
                context.commit('setTimelineCreating', false)
            }
            context.commit("removeEvent", data)
        },

        /** Comments */
        addLocationComment: function (context, data) {
            axios.post("/Trip/AddLocationComment/" + TRIP_ID, data)
            .then(function (response) {
                if (response.data !== null && response.data !== null){ 
                    context.commit('addLocationComment', response.data);
                }
            }).catch(function(info) {
                console.log(info)
            });
        },
        addEventComment: function (context, data) {
            axios.post("/Trip/AddEventComment/" + TRIP_ID, {
                eventId: data.eventId,
                comment: data.comment
            })
            .then(function (response) {
                if (response.data !== null && response.data !== null){ 
                    response.data.locationId = data.locationId
                    context.commit('addEventComment', response.data);
                }
            }).catch(function(info) {
                console.log(info)
            });
        },
        deleteComment: function (context, data) {
            axios.post("/Trip/RemoveComment/" + TRIP_ID, {
                commentId: data.id
            })
            .then(function (response) {
                console.log(response)
            }).catch(function(info) {
                console.log(info)
            });
        },

        /** Photos (add operation proceeds by dropbox) */
        deleteLocationPhoto: function (context, data) {
            axios.post("/Trip/DeletePhoto/" + TRIP_ID, data.photo)
            .then(function (response) {
                console.log(response)
                context.commit('deleteLocationPhoto', data);
            }).catch(function(info) {
                console.log(info)
            });
        },
        deleteEventPhoto: function (context, data) {
            axios.post("/Trip/DeletePhoto/" + TRIP_ID, data.photo)
            .then(function (response) {
                console.log(response)
                context.commit('deleteEventPhoto', data);
            }).catch(function(info) {
                console.log(info)
            });
        },

        /** Stars */
        locationStarred: function (context, data) {
            if (data.location.starredValue !== null) return;

            axios.post("/Trip/AddLocationStarRecord/" + TRIP_ID, {
                value: data.value,
                locationId: data.location.id
            }).then(function (response) {
                console.log(response)
            }).catch(function(info){
                console.log(info)
            })

            context.commit("setLocationStars", data);
        },
        eventStarred: function (context, data) {
            if (data.event.starredValue !== null) return;

            axios.post("/Trip/AddEventStarRecord/" + TRIP_ID, {
                value: data.value,
                eventId: data.event.id
            }).then(function (response) {
                console.log(response)
            }).catch(function(info){
                console.log(info)
            })

            context.commit("setEventStars", data);
        }
    },
    mutations: {
        setTrip: function (state, trip) {
            state.trip = trip
        },
        setTripDescription: function (state, description) {
            state.trip.description = description
        },
        setTimelineCreating: function (state, value){
            state.timelineCreating = value
        },
        setTripStars: function (state, value) {
            state.trip.starredValue = { value: value }
            state.trip.starsCount = state.trip.starsCount + 1
        },
        setLocationStars: function (state, data) {
            data.location.starredValue = { value: data.value }
        },
        setEventStars: function (state, data) {
            data.event.starredValue = { value: data.value }
        },
        addDefaultLocation: function (state, newLocation) {
            var lastLocationPositionIndex = 0;
            for (var i = 0; i < state.trip.locations.length; i++) {
                if (state.trip.locations[i].position > lastLocationPositionIndex)
                    lastLocationPositionIndex = state.trip.locations[i].position
            }

            newLocation.position = lastLocationPositionIndex + 1
            newLocation.events = []
            newLocation.photos = []
            newLocation.comments = []
            newLocation.mark = "new"
            state.trip.locations.push(newLocation)
        },
        addDefaultEvent: function (state, newEvent){
            if (typeof state.trip.locations == 'undefined') return;

            var locationIndex = state.trip.locations.length - 1;
            if (locationIndex < 0) return;

            if (typeof state.trip.locations[locationIndex].events == 'undefined') {
                state.trip.locations[locationIndex].events = []
            }

            var lastEventPositionIndex = 0;
            for (var i = 0; i < state.trip.locations[locationIndex].events.length; i++) {
                var position = state.trip.locations[locationIndex].events[i].position;
                if (position > lastEventPositionIndex)
                    lastEventPositionIndex = position
            }

            newEvent.position = lastEventPositionIndex + 1
            newEvent.locationId = state.trip.locations[locationIndex].id
            newEvent.photos = []
            newEvent.comments = []
            newEvent.mark = "new"
            state.trip.locations[locationIndex].events.push(newEvent)
        },
        removeLocation: function (state, location) {
            var locArrayIndex = -1;
            for (var i = 0; i < state.trip.locations.length; i++) {
                if (state.trip.locations[i] == location)
                    locArrayIndex = i;
            }

            if (locArrayIndex < 0) return;

            state.trip.locations.splice(locArrayIndex, 1)

            var position = 1
            for (var i = 0; i < state.trip.locations.length; i++) {
                state.trip.locations[i].position = position;
                position++;                
            }
        },
        removeEvent: function (state, event) {
            if (typeof event.locationId == 'undefined' || event.locationId == null) return;

            var locArrayIndex = -1;
            for (var i = 0; i < state.trip.locations.length; i++) {
                if (state.trip.locations[i].id == event.locationId) {}
                    locArrayIndex = i;
            }
            if (locArrayIndex < 0) return;

            var evArrayIndex = -1;
            for (var i = 0; i < state.trip.locations[locArrayIndex].events.length; i++) {
                if (state.trip.locations[locArrayIndex].events[i] == location) {}
                    evArrayIndex = i;
            }
            if (evArrayIndex < 0) return;

            state.trip.locations[locArrayIndex].events.splice(evArrayIndex, 1)

            var position = 1
            for (var i = 0; i < state.trip.locations[locArrayIndex].events.length; i++) {
                state.trip.locations[locArrayIndex].events[i].position = position;
                position++;                
            }
        },
        addLocationComment: function (state, data) {
            for (var i = 0; i < state.trip.locations.length; i++) {
                if (state.trip.locations[i].id == data.location.id) {
                    if (typeof state.trip.locations[i].comments == "undefined" ||
                        state.trip.locations[i].comments == null) {
                        state.trip.locations[i].comments = []
                    }

                    delete data.location;

                    state.trip.locations[i].comments.push(data)
                    break;
                }                
            }
        },
        addEventComment: function (state, data) {
            var location = null;
            for (var i = 0; i < state.trip.locations.length; i++) {
                if (state.trip.locations[i].id == data.locationId) {
                    location = state.trip.locations[i]
                    break;
                }
            }

            if (location == null) return;

            for (var j = 0; j < location.events.length; j++) {
                if (location.events[j].id == data.event.id) {
                    if (typeof location.events[j].comments == "undefined" ||
                        location.events[j].comments == null) {
                        location.events[j].comments = []
                    }

                    delete data.event;
                    delete data.locationId;

                    location.events[j].comments.push(data)
                    break;
                }
            }
        },
        addGalleryPhotos: function (state, photos) {
            for (var i = 0; i < photos.length; i++) {
                state.trip.photos.push(photos[i])
            }
        },
        setPhotoBasic: function (state, photo) {
            for (var i = 0; i < state.trip.photos.length; i++) {
                state.trip.photos[i].isBasic = false
            }
            photo.isBasic = true
        },
        deletePhoto: function (state, photo) {
            var photoIndex = -1
            for (var i = 0; i < state.trip.photos.length; i++) {
                if (state.trip.photos[i] == photo) {
                    photoIndex = i
                    break;
                }
            }

            if (photoIndex >= 0) {
                state.trip.photos.splice(photoIndex, 1)
            }
        },
        addLocationPhotos: function (state, data) {
            for (var i = 0; i < state.trip.locations.length; i++) {
                if (state.trip.locations[i].id == data.locationId) {
                    if (typeof state.trip.locations[i].photos == "undefined" ||
                        state.trip.locations[i].photos == null) {
                        state.trip.locations[i].photos = []
                    }

                    for (var j = 0; j < data.photos.length; j++) {
                        state.trip.locations[i].photos.push(data.photos[j])
                    }

                    break;
                }                
            }
        },
        deleteLocationPhoto: function (state, data) {
            if ($.isArray(data.location.photos)) {
                var photoIndex = -1
                for (var i = 0; i < data.location.photos.length; i++) {
                    if (element = data.location.photos[i] == data.photo) {
                        photoIndex = i
                        break;
                    }
                }
                if (photoIndex >= 0) {
                    data.location.photos.splice(photoIndex, 1)
                }
            }
        },
        addEventPhotos: function (state, data) {
            for (var j = 0; j < state.trip.locations.length; j++) {
                if (state.trip.locations[j].id == data.locationId) {
                    var events = state.trip.locations[j].events

                    for (var i = 0; i < events.length; i++) {
                        if (events[i].id == data.eventId) {
                            if (typeof events[i].photos == "undefined" ||
                                events[i].photos == null) {
                                events[i].photos = []
                            }

                            for (var j = 0; j < data.photos.length; j++) {
                                events[i].photos.push(data.photos[j])
                            }
                            break;
                        }                
                    }
                    break;
                }
            }
        },
        deleteEventPhoto: function (state, data) {
            if ($.isArray(data.event.photos)) {
                var photoIndex = -1
                for (var i = 0; i < data.event.photos.length; i++) {
                    if (element = data.event.photos[i] == data.photo) {
                        photoIndex = i
                        break;
                    }
                }
                if (photoIndex >= 0) {
                    data.event.photos.splice(photoIndex, 1)
                }
            }
        }
    }
})

/*--------------------------------------------------------*/
var descriptionInstance = null;
var galleryDropzoneInstance = null;
var locationSortableInstance = null;
var defaultLocation = {
    model: "location",
    IsPublic: true,
    Stars: 0,
    Description: "Введите описание...",
    PlaceTitle: "Выберите местоположение",
    Address: "",
    Country: "",
    State: "",
    City: "",
    Latitude: "",
    Longitude: "",
    ArrivalDate: ""
}
var defaultEvent = {
    model: "event",
    IsPublic: true,
    Stars: 0,
    Title: "Новое событие",
    Description: "Введите описание события...",
    EventType: "entertainment",
    Address: "",
    Latitude: "",
    Longitude: "",
    EventDate: ""
}
var defaultData = {
    trip: {
        viewType: "overview", // edit, overview
        id: 0,
        isPublic: true,
        title: "Название поездки",
        starsAvg: 4,
        starsCount: 504,
        description: "kekek",
        dateAdd: '18.04.2017 18:36',
        photos: [
            { url: '/images/templatemo_slide_1.jpg' },
            { url: '/images/templatemo_slide_2.jpg' },
            { url: '/images/templatemo_slide_3.jpg' },
            { url: '/images/templatemo_slide_1.jpg' },
            { url: '/images/templatemo_slide_2.jpg' },
            { url: '/images/templatemo_slide_3.jpg' }
        ]
    },
    timeline: [{
        model: "location",
        id: 1,
        isPublic: true,
        starsAvg: 5,
        position: 1,
        description: "Description",
        placeTitle: "Russia, Moscow",
        address: "Russia, Moscow",
        country: "Russia",
        state: "Moscow",
        city: "Moscow",
        latitude: "",
        longitude: "",
        photos: [
            { url: '/images/templatemo_slide_1.jpg' },
            { url: '/images/templatemo_slide_2.jpg' },
            { url: '/images/templatemo_slide_3.jpg' },
            { url: '/images/templatemo_slide_1.jpg' },
            { url: '/images/templatemo_slide_2.jpg' },
            { url: '/images/templatemo_slide_3.jpg' }
        ],
        arrivalDate: "18.04.2017",
        dateAdd: "18.04.2017 18:36",
        dateUpdate: "18.04.2017 18:36",
        events: [{
            model: "event",
            Id: 1,
            isPublic: true,
            starsAvg: 5,
            position: 2,
            title: "Event",
            description: "Description",
            eventType: "entertainment",
            address: "Russia, Moscow",
            latitude: "",
            longitude: "",
            eventDate: "18.04.2017",
            dateAdd: "18.04.2017 18:36",
            dateUpdate: "18.04.2017 18:36",
            photos: [
                { url: '/images/templatemo_slide_1.jpg' },
                { url: '/images/templatemo_slide_2.jpg' },
                { url: '/images/templatemo_slide_3.jpg' },
                { url: '/images/templatemo_slide_1.jpg' },
                { url: '/images/templatemo_slide_2.jpg' },
                { url: '/images/templatemo_slide_3.jpg' }
            ]
        }]
    }]
}
var placesTypes = [
    { value: "jewelry_store", text: "Ювелирный магазин" },
    { value: "library", text: "Библиотека" },
    { value: "liquor_store", text: "Спиртные напитки" },
    { value: "lodging", text: "Жилье" },
    { value: "mosque", text: "Мечеть" },
    { value: "movie_theater", text: "Кинотеатр" },
    { value: "museum", text: "Музей" },
    { value: "night_club", text: "Ночной клуб" },
    { value: "park", text: "Парк" },
    { value: "restaurant", text: "Ресторан" },
    { value: "shoe_store", text: "Обувной магазин" },
    { value: "shopping_mall", text: "Торговый центр" },
    { value: "spa", text: "СПА" },
    { value: "stadium", text: "Стадион" },
    { value: "store", text: "Магазин" },
    { value: "subway_station", text: "Метро" },
    { value: "synagogue", text: "Синагога" },
    { value: "train_station", text: "ЖД вокзал" },
    { value: "university", text: "Университет" },
    { value: "zoo", text: "Зоопарк" },
    { value: "airport", text: "Аэропорт" },
    { value: "amusement_park", text: "Парк развлечений" },
    { value: "aquarium", text: "Аквариум" },
    { value: "art_gallery", text: "Галерея искусств" },
    { value: "bar", text: "Бар" },
    { value: "beauty_salon", text: "Салон красоты" },
    { value: "bicycle_store", text: "Магазин велосипедов" },
    { value: "book_store", text: "Книжный магазин" },
    { value: "bowling_alley", text: "Боулинг" },
    { value: "bus_station", text: "Автобусная остановка" },
    { value: "cafe", text: "Кафе" },
    { value: "campground", text: "Палаточный лагерь" },
    { value: "casino", text: "Казино" },
    { value: "church", text: "Церковь" },
    { value: "clothing_store", text: "Магазин одежды" },
    { value: "gym", text: "Гимнастический зал" }
]
/*--------------------------------------------------------*/

var starsComponent = {
    template: "#stars-component",
    props: ["value", "enabled"],
    data: function () {
        return {
            starsTemp: null
        }
    },
    watch: {
        value: function (v) {
            this.revealStarsTempCount();
        }
    },
    methods: {
        setStarsCount: function (count) {
            if (!this.enabled) return
            this.starsTemp = count;
            this.$emit("starred", count)
        },
        revealStarsTempCount: function () {
            this.starsTemp = this.value
        },
        setStarsTempCount: function (count) {
            if (!this.enabled) return
            this.starsTemp = count
        },
    },
    created: function () {
        this.revealStarsTempCount()
    }
}

var commentComponent = {
    template: "#comment-component",
    props: ['model', 'viewType'],
    methods: {
        deleteComment: function () {
            this.$emit('deleteComment', this.model)
        }
    }
}

var tripPageHeading = {
    template: "#tripPage-heading",
    props: ["trip", "tabs"],
    data: function () {
        return {
            'allowEditing': window.ALLOW_EDITING,
            'authenticated': window.AUTHENTICATED
        }
    },
    components: {
        "stars-component": starsComponent
    },
    computed: {
        starValue: function () {
            return typeof this.trip.starredValue !== 'undefined' && this.trip.starredValue !== null ? this.trip.starredValue.value : this.trip.starsAvg 
        },
        starringEnabled: function () {
            return !this.authenticated || typeof this.trip.starredValue !== 'undefined' && this.trip.starredValue !== null ? false : true
        }
    },
    methods: {
        toEditMode: function () {
            this.$emit("viewTypeChanged", "toEditMode")
        },
        saveChanges: function () {
            this.$emit("viewTypeChanged", "saveChanges")
        },
        tabSelected: function (tab) {
            for (var i = 0; i < this.tabs.length; i++) {
                this.tabs[i].active = false;
            }
            tab.active = true;
        },
        starred: function (value) {
            this.$store.dispatch("tripStarred", value)
        },
        deleteTrip: function () {
            if (confirm("Вы уверены, что хотите удалить записи о поездке?")) {
                this.$store.dispatch("deleteTrip")
            }
        }
    }
};

var timelineEvent = {
    template: "#timeline-event",
    props: ["model", "locationAddress", "locationGeoPosition"],
    data: function () {
        return {
            viewType: 'overview',
            commentText: "",
            allowEditing: window.ALLOW_EDITING,
            authenticated: window.AUTHENTICATED,
            descriptionOpened: false,
            tabs: null,
            dropzoneInstance: null,

            mapInstance: null,
            placesInstance: null,
            geocoderInstance: null,
            mapMarker: null,
            placesLocationAddress: null,
            placesTypesSelected: [],
            placesDistanceSelected: "1000",
            placesSearchResult: [],
            locationAddressLocal: "",
            useLocalLocationAddress: false
        }
    },
    components: {
        "stars-component": starsComponent,
        "timeline-event": timelineEvent,
        "comment-component": commentComponent
    },
    computed: {
        selectedTab: function () {
            if (this.tabs !== null)
                for (var i = 0; i < this.tabs.length; i++) {
                    if (this.tabs[i].active) return this.tabs[i].type
                }
            return "";
        },
        starValue: function () {
            return typeof this.model.starredValue !== 'undefined' && this.model.starredValue !== null ? this.model.starredValue.value : this.model.starsAvg 
        },
        starringEnabled: function () {
            return !this.authenticated || typeof this.model.starredValue !== 'undefined' && this.model.starredValue !== null ? false : true
        },
        placesTypes: function () {
            return placesTypes
        },
        staticMapUrl: function () {
            var center = this.model.latitude.toString().concat(",").concat(this.model.longitude.toString())
            return "https://maps.googleapis.com/maps/api/staticmap?" + $.param({
                center: center.toString(),
                zoom: this.model.mapZoom,
                size: "638x250",
                maptype: "roadmap",
                markers: "size:mid|" + center.toString(),
                key: "AIzaSyCRagVXBCWgvRlx-Tn5U5xnquR7ikJzELA"
            })
        },
        placesDistances: function () {
            return [
                { text: "100 м", value: "100" },
                { text: "500 м", value: "500" },
                { text: "1 км", value: "1000" },
                { text: "2 км", value: "2000" },
                { text: "5 км", value: "5000" },
                { text: "10 км", value: "5000" },
                { text: "20 км", value: "5000" }
            ]
        }
    },
    watch: {
        viewType: function (value) {
            this.setTabsList(value)
            if (value == 'edit') this.initEditMap()
        },
        model: function (value) {
            if (typeof value.mark == 'undefined' || 
                typeof value.mark !== 'undefined' && value.mark !== 'new') {
                this.setTabsList(this.viewType)
            }
        },
        locationAddress: function (value) {
            if (!this.useLocalLocationAddress)
                this.locationAddressLocal = this.locationAddress
        },
        useLocalLocationAddress: function (value) {
            if (!value) {
                this.locationAddressLocal = this.locationAddress
                this.placesSearch()
            }
        },
        placesTypesSelected: function () {
            this.placesSearch()
        },
        placesDistanceSelected: function () {
            this.placesSearch()
        }
    },
    methods: {
        addComment: function () {
            var comment = this.commentText
            this.$store.dispatch('addEventComment', {
                locationId: this.model.locationId,
                eventId: this.model.id,
                comment: comment
            });
            this.commentText = ""
        },
        deleteComment: function (comment) {
            this.$store.dispatch('deleteComment', comment)

            var toDelete = -1;
            for (var i = 0; i < this.model.comments.length; i++) {
                if (this.model.comments[i] == comment) {
                    toDelete = i;
                    break
                }
            }

            if (toDelete >= 0) {
                this.model.comments.splice(toDelete, 1)
            }
        },
        descriptionOpenedToggle: function () {
            this.descriptionOpened = !this.descriptionOpened
        },
        tabSelected: function (tab) {
            for (var i = 0; i < this.tabs.length; i++) {
                this.tabs[i].active = false;
            }
            tab.active = true;

            if (typeof tab.onShow == 'function') {
                tab.onShow()
            }
        },
        setTabsList: function (viewType) {
            var that = this;

            var overviewTabs = [{
                    title: "основное",
                    active: true,
                    type: "home"
                },{
                    title: "на карте",
                    active: false,
                    type: "map"
                },
                {
                    title: "описание",
                    active: false,
                    type: "description"
                },
                {
                    title: "галерея",
                    active: false,
                    type: "gallery"
                }
            ];
            var editTabs = [{
                    title: "основное",
                    active: true,
                    type: "edit-main"
                },
                {
                    title: "рекомендации",
                    active: false,
                    type: "edit-recomendations",
                    onShow: function () {
                        if (that.mapInstance !== null) {
                            that.placesSearch()
                        }
                    }
                },
                {
                    title: "на карте",
                    active: false,
                    type: "edit-map",
                    onShow: function () {
                        if (that.mapInstance !== null) {
                            setTimeout(function () {
                                google.maps.event.trigger(that.mapInstance, 'resize');
                            }, 100)
                        }
                    }
                },
                {
                    title: "описание",
                    active: false,
                    type: "edit-description"
                }
            ];

            if (typeof this.model.mark == 'undefined' || 
                typeof this.model.mark !== 'undefined' && this.model.mark !== 'new') {
                editTabs.push({
                    title: "комментарии",
                    active: false,
                    type: "edit-comments"
                });
                editTabs.push({
                    title: "галерея",
                    active: false,
                    type: "edit-gallery"
                });
            }

            this.tabs = viewType == 'edit' ? editTabs : overviewTabs;
        },
        toEditViewType: function () {
            this.viewType = 'edit'
        },
        saveEvent: function () {
            if (this.mapInstance !== null)
                this.model.mapZoom = this.mapInstance.getZoom()

            var data = this.model;
            data.description = $(this.$refs.description).summernote('code');

            if (this.dropzoneInstance !== null)
                this.dropzoneInstance.processQueue();

            if (typeof this.model.mark !== 'undefined' && this.model.mark == 'new') {
                this.$store.dispatch('createEvent', data)
                this.viewType = 'overview'
                return;
            }

            this.$store.dispatch('saveEvent', data);
            this.viewType = 'overview'
        },
        deleteEvent: function () {
            this.$store.dispatch('deleteEvent', this.model)
        },
        deletePhoto: function (photo) {
            this.$store.dispatch('deleteEventPhoto', {
                event: this.model,
                photo: photo
            })
        },
        initDropzone: function () {
            var that = this

            this.dropzoneInstance = new Dropzone(this.$refs.dropzone, 
            {
                url: "/Trip/AddEventPhotos/" + TRIP_ID,
                paramName: 'photos',
                uploadMultiple: true,
                addRemoveLinks: true,
                autoProcessQueue: false,
                acceptedFiles: "image/*",
                parallelUploads: 5,
                maxFiles: 5
            });

            this.dropzoneInstance.on('successmultiple', function (files, response) {
                if (typeof response.eventId !== "undefined" &&
                    typeof response.photos !== "undefined" &&
                    $.isArray(response.photos)) {
                    that.$store.commit('addEventPhotos', response);
                }
            })

            this.dropzoneInstance.on('sendingmultiple', function (files, xhr, data) {
                data.append("eventId", that.model.id)
            })

            this.dropzoneInstance.on("completemultiple", function(files) {
                for (var i = 0; i < files.length; i++) {
                    that.dropzoneInstance.removeFile(files[i]);
                }
            });
        },
        starred: function (value) {
            this.$store.dispatch("eventStarred", {
                event: this.model,
                value: value
            })
        },

        initEditMap: function () {
            if (this.mapInstance == null) {
                var lat = this.model.latitude == null ? 0 : this.model.latitude;
                var lng = this.model.longitude == null ? 0 : this.model.longitude;
                var zoom = this.model.mapZoom == null ? 5 : this.model.mapZoom;

                this.mapInstance = new google.maps.Map(this.$refs.editMap, {
                    center: { lat: lat, lng: lng }, zoom: zoom
                })

                this.mapMarker = new google.maps.Marker({
                    map: this.mapInstance,
                    position: { lat: lat, lng: lng }
                });

                this.placesInstance = new google.maps.places.PlacesService(this.mapInstance);
                this.geocoderInstance = new google.maps.Geocoder();
            }
        },
        placesSearch: function () {
            var that = this;

            if (this.placesInstance == null) return
            if (this.placesTypesSelected.length == 0) return

            if (this.useLocalLocationAddress) {
                this.geocoderSearch(
                    this.locationAddressLocal,
                    function (location) {
                        if (location !== null) {
                            that.placesSearchFn(
                                location, 
                                that.placesDistanceSelected, 
                                that.placesTypesSelected
                            )
                        } else {
                            that.placesSearchResult = []
                        }
                    }
                )
            } else {
                this.placesSearchFn(
                    this.locationGeoPosition, 
                    this.placesDistanceSelected, 
                    this.placesTypesSelected
                )
            }
        },
        geocoderSearch: function (address, callback) {
            var geocoder = this.geocoderInstance;
            var that = this;

            if (geocoder == null) {
                console.log("Something went wrong");
                return;
            } 

            geocoder.geocode( { 'address': address }, function(results, status) {
                if (status == 'OK') {
                    callback(results[0].geometry.location)
                } else {
                    callback(null)
                    console.log('Geocode was not successful for the following reason: ' + status);
                }
            });
        },
        placesSearchFn: function (location, radius, types) {
            var that = this;
            var request = {
                location: location,
                radius: radius,
                types: types
            };

            this.placesInstance.nearbySearch(request, function (results, status) {
                if (status == "OK") {
                    that.placesSearchResult = results
                } else {
                    that.placesSearchResult = []
                    console.log("Something went wrong", status)
                }
            });
        },
        choosePlace: function (place) {
            this.model.title = place.name
            this.model.address = place.vicinity
            this.model.latitude = place.geometry.location.lat()
            this.model.longitude = place.geometry.location.lng()
            this.model.mapZoom = 8

            this.mapInstance.setCenter(place.geometry.location)
            this.mapInstance.setZoom(8)

            if (this.mapMarker !== null) {
                this.mapMarker.setMap(null)
                this.mapMarker = null
            }
            this.mapMarker = new google.maps.Marker({
                map: this.mapInstance,
                position: place.geometry.location
            });
        }
    },
    created: function () {
        this.setTabsList(this.viewType)
        if (typeof this.model.mark !== 'undefined' && this.model.mark == 'new')
            this.toEditViewType()
        
        this.locationAddressLocal = this.locationAddress

        switch(this.model.eventType) {
            case "hotel": this.placesTypesSelected = ["lodging"]; break;
            case "entertainment": this.placesTypesSelected = ["zoo", "stadium", "amusement_park", "aquarium", "bowling_alley"]; break;
            case "interesting": this.placesTypesSelected = ["museum", "park", "gallery"]; break;
            case "note": this.placesTypesSelected = []; break;
            default: this.placesTypesSelected = []; break;
        }
    },
    mounted: function () {
        $(this.$refs.description).summernote({
            height: 200,
            minHeight: 50,
            maxHeight: 600
        });

        if (this.viewType == 'edit') {
            this.initEditMap()
        }

        $(this.$refs.placeTypes).select2({
            placeholder: 'Типы мест'
        })

        this.initDropzone()
    }
}

var timelineLocation = {
    template: "#timeline-location",
    props: ["model", "pageViewType"],
    data: function () {
        return {
            commentText: "",
            geocoderSearchQuery: "",
            viewType: 'overview',
            descriptionOpened: false,
            allowEditing: window.ALLOW_EDITING,
            authenticated: window.AUTHENTICATED,
            tabs: null,
            sortableInstance: null,
            dropzoneInstance: null,
            mapInstance: null,
            geocoderInstance: null,
            geocoderSearchResult: null,
            geocoderMarker: null
        }
    },
    components: {
        "stars-component": starsComponent,
        "timeline-event": timelineEvent,
        "comment-component": commentComponent
    },
    computed: {
        selectedTab: function () {
            if (this.tabs !== null)
                for (var i = 0; i < this.tabs.length; i++) {
                    if (this.tabs[i].active) return this.tabs[i].type
                }
            return "";
        },
        eventsSorted: function () {
            if (typeof this.model !== "undefined" &&  this.model !== null &&
                typeof this.model.events !== "undefined" && this.model.events !== null) {
                return this.model.events.sort(function (a, b) {
                    if (a.position < b.position) return -1
                    if (a.position > b.position) return 1
                    return 0
                })
            } else return []
        },
        starValue: function () {
            return typeof this.model.starredValue !== 'undefined' && this.model.starredValue !== null ? this.model.starredValue.value : this.model.starsAvg 
        },
        starringEnabled: function () {
            return !this.authenticated || typeof this.model.starredValue !== 'undefined' && this.model.starredValue !== null ? false : true
        },
        staticMapUrl: function () {
            var center = this.model.latitude.toString().concat(",").concat(this.model.longitude.toString())
            return "https://maps.googleapis.com/maps/api/staticmap?" + $.param({
                center: center.toString(),
                zoom: this.model.mapZoom,
                size: "638x250",
                maptype: "roadmap",
                markers: "size:mid|" + center.toString(),
                key: "AIzaSyCRagVXBCWgvRlx-Tn5U5xnquR7ikJzELA"
            })
        },
        locationGeoPosition: function () {
            return {
                lat: this.model.latitude,
                lng: this.model.longitude
            }
        }
    },
    watch: {
        viewType: function (value) {
            this.setTabsList(value)
            if (value == 'edit') this.initEditMap()
        },
        pageViewType: function (value) {
            if(value == 'edit') {
                this.setUpEventsSortable()
            } else {
                this.destroyEventsSortable()
            }
        },
        model: function (value) {
            if (typeof value.mark == 'undefined' || 
                typeof value.mark !== 'undefined' && value.mark !== 'new') {
                this.setTabsList(this.viewType)
            }
        }
    },
    methods: {
        addComment: function () {
            var comment = this.commentText
            this.$store.dispatch('addLocationComment', {
                locationId: this.model.id,
                comment: comment
            });
            this.commentText = ""
        },
        deleteComment: function (comment) {
            this.$store.dispatch('deleteComment', comment)

            var toDelete = -1;
            for (var i = 0; i < this.model.comments.length; i++) {
                if (this.model.comments[i] == comment) {
                    toDelete = i;
                    break
                }
            }

            if (toDelete >= 0) {
                this.model.comments.splice(toDelete, 1)
            }
        },
        descriptionOpenedToggle: function () {
            this.descriptionOpened = !this.descriptionOpened
        },
        tabSelected: function (tab) {
            for (var i = 0; i < this.tabs.length; i++) {
                this.tabs[i].active = false;
            }
            tab.active = true;

            if (typeof tab.onShow == 'function') {
                tab.onShow()
            }
        },
        setTabsList: function (viewType) {
            var that = this;

            var overviewTabs = [{
                    title: "основное",
                    active: true,
                    type: "home"
                },{
                    title: "на карте",
                    active: false,
                    type: "map"
                },
                {
                    title: "описание",
                    active: false,
                    type: "description"
                },
                {
                    title: "галерея",
                    active: false,
                    type: "gallery"
                }
            ];
            var editTabs = [{
                    title: "основное",
                    active: true,
                    type: "edit-main"
                },
                {
                    title: "на карте",
                    active: false,
                    type: "edit-map",
                    onShow: function () {
                        if (that.mapInstance !== null) {
                            setTimeout(function () {
                                google.maps.event.trigger(that.mapInstance, 'resize');
                            }, 100)
                        }
                    }
                },
                {
                    title: "описание",
                    active: false,
                    type: "edit-description"
                }
            ];

            if (typeof this.model.mark == 'undefined' || 
                typeof this.model.mark !== 'undefined' && this.model.mark !== 'new') {
                editTabs.push({
                    title: "комментарии",
                    active: false,
                    type: "edit-comments"
                });
                editTabs.push({
                    title: "галерея",
                    active: false,
                    type: "edit-gallery"
                });
            }

            this.tabs = viewType == 'edit' ? editTabs : overviewTabs;
        },
        toEditViewType: function () {
            this.viewType = 'edit'
        },
        saveLocation: function () {
            var data = this.model;
            data.description = $(this.$refs.description).summernote('code');

            if (this.dropzoneInstance !== null)
                this.dropzoneInstance.processQueue();

            if (typeof this.model.mark !== 'undefined' && this.model.mark == 'new') {
                this.$store.dispatch('createLocation', data)
                this.viewType = 'overview'
                return;
            }

            this.$store.dispatch('saveLocation', data);
            this.viewType = 'overview'
        },
        deleteLocation: function () {
            this.$store.dispatch('deleteLocation', this.model)
        },
        setUpEventsSortable: function () {
            this.sortableInstance = Sortable.create(this.$refs.events, {
                draggable: ".timeline-event"
            });
        },
        destroyEventsSortable: function () {
            if (this.sortableInstance !== null)
                this.sortableInstance.destroy()
        },
        deletePhoto: function (photo) {
            this.$store.dispatch('deleteLocationPhoto', {
                location: this.model,
                photo: photo
            })
        },
        initDropzone: function () {
            var that = this

            this.dropzoneInstance = new Dropzone(this.$refs.dropzone, 
            {
                url: "/Trip/AddLocationPhotos/" + TRIP_ID,
                paramName: 'photos',
                uploadMultiple: true,
                addRemoveLinks: true,
                autoProcessQueue: false,
                acceptedFiles: "image/*",
                parallelUploads: 5,
                maxFiles: 5
            });

            this.dropzoneInstance.on('successmultiple', function (files, response) {
                if (typeof response.locationId !== "undefined" &&
                    typeof response.photos !== "undefined" &&
                    $.isArray(response.photos)) {
                    that.$store.commit('addLocationPhotos', response);
                }
            })

            this.dropzoneInstance.on('sendingmultiple', function (files, xhr, data) {
                data.append("locationId", that.model.id)
            })

            this.dropzoneInstance.on("completemultiple", function(files) {
                for (var i = 0; i < files.length; i++) {
                    that.dropzoneInstance.removeFile(files[i]);
                }
            });
        },
        starred: function (value) {
            this.$store.dispatch("locationStarred", {
                location: this.model,
                value: value
            })
        },
        initEditMap: function () {
            if (this.mapInstance == null) {
                var lat = this.model.latitude == null ? 0 : this.model.latitude;
                var lng = this.model.longitude == null ? 0 : this.model.longitude;
                var zoom = this.model.mapZoom == null ? 8 : this.model.mapZoom;

                this.mapInstance = new google.maps.Map(this.$refs.editMap, {
                    center: { lat: lat, lng: lng }, zoom: zoom
                })

                this.geocoderMarker = new google.maps.Marker({
                    map: this.mapInstance,
                    position: { lat: lat, lng: lng }
                });

                this.geocoderInstance = new google.maps.Geocoder();
            }
        },
        geocoderSearch: function () {
            var address = this.geocoderSearchQuery;
            var map = this.mapInstance;
            var geocoder = this.geocoderInstance;
            var that = this;

            if (map == null || geocoder == null) {
                console.log("Something went wrong");
                return;
            } 

            geocoder.geocode( { 'address': address }, function(results, status) {
                if (status == 'OK') {
                    that.geocoderSearchResult = results[0]
                    map.setCenter(results[0].geometry.location);
                    if (that.geocoderMarker !== null) {
                        that.geocoderMarker.setMap(null)
                        that.geocoderMarker = null
                    }
                    that.geocoderMarker = new google.maps.Marker({
                        map: map,
                        position: results[0].geometry.location
                    });
                } else {
                    that.geocoderSearchResult = null
                    console.log('Geocode was not successful for the following reason: ' + status);
                }
            });
        },
        applyGeocoderResult: function () {
            if (this.geocoderSearchResult !== null) {
                this.model.placeTitle = this.geocoderSearchQuery
                this.model.latitude = this.geocoderSearchResult.geometry.location.lat()
                this.model.longitude = this.geocoderSearchResult.geometry.location.lng()
                this.model.address = this.geocoderSearchResult.formatted_address

                var country = ""
                var state = ""
                var city = ""

                for (var i = 0; i < this.geocoderSearchResult.address_components.length; i++) {
                    var component = this.geocoderSearchResult.address_components[i];
                    if (component.types.indexOf("country") > -1 && country == "") {
                        country = component.long_name
                        continue
                    }

                    if (component.types.indexOf("administrative_area_level_1") > -1 && 
                        component.types.indexOf("political") > -1 &&
                        state == "") {
                        state = component.long_name
                        continue
                    }

                    if (component.types.indexOf("locality") > -1 && 
                        component.types.indexOf("political") > -1 &&
                        city == "") {
                        city = component.long_name
                        continue
                    }
                }

                this.model.country = country
                this.model.state = state
                this.model.city = city
                this.model.mapZoom = this.mapInstance.getZoom()
            }
        }
    },
    created: function () {
        this.setTabsList(this.viewType)
        if (typeof this.model.mark !== 'undefined' && this.model.mark == 'new')
            this.toEditViewType()

        this.geocoderAddress = this.model.placeTitle
    },
    mounted: function () {
        if (this.pageViewType == 'edit') {
            this.setUpEventsSortable()
        }

        if (this.viewType == 'edit') {
            this.initEditMap()
        }

        $(this.$refs.description).summernote({
            height: 200,
            minHeight: 50,
            maxHeight: 600
        });
        
        this.initDropzone()
    }
}

var tripPageTimeline = {
    template: "#tripPage-timeline",
    props: ["timeline", "viewType"],
    data: function () {
        return {
            'allowEditing': window.ALLOW_EDITING
        }
    },
    components: {
        "timeline-location": timelineLocation
    },
    computed: Vuex.mapState({
        timelineCreating: 'timelineCreating',
        timelineSorted: function () {
            if (typeof this.timeline !== "undefined" &&  this.timeline !== null) {
                return this.timeline.sort(function (a, b) {
                    if (a.position < b.position) return -1
                    if (a.position > b.position) return 1
                    return 0
                })
            } else return []
        }
    }),
    watch: {
        viewType: function (value) {
            if(value == 'edit') {
                this.setUpLocationsSortable()
            } else {
                this.destroyLocationsSortable()
            }
        }
    },
    methods: {
        setUpLocationsSortable: function () {
            locationSortableInstance = Sortable.create(this.$refs.timeline, {
                draggable: ".timeline-location"
            });
        },
        destroyLocationsSortable: function () {
            if (locationSortableInstance != null)
                locationSortableInstance.destroy()
        },
        addLocation: function () {
            this.$emit('addLocation')
        },
        addEvent: function (type) {
            this.$emit('addEvent', type)
        }
    },
    mounted: function () {
        if (this.viewType == 'edit') {
            this.setUpLocationsSortable()
        }
    }
}

var tripPageDescription = {
    template: "#tripPage-description",
    props: ["description", "viewType"],
    data: function () {
        return {
            initialized: true
        }
    },
    watch: {
        viewType: function (value) {
            if(value == 'edit') {
                this.setUpEditor()
            } else {
                this.destroyEditor()
            }
        }
    }, 
    methods: {
        setUpEditor: function () {
            this.initialized = true
            $(this.$refs.description).summernote({
                height: 300,
                minHeight: 150,
                maxHeight: 600
            });
        },
        destroyEditor: function () {
            var data = $(this.$refs.description).summernote('code')
            $(this.$refs.description).summernote('destroy')
            this.$emit('editorDestroyed', data)
        }
    },
    mounted: function () {
        if (this.viewType == 'edit') {
            this.setUpEditor();
        }
        descriptionInstance = this.$refs.description
    }
}

var tripPageGallery = {
    template: "#tripPage-gallery",
    props: ["photos", "viewType"],
    methods: {
        setPhotoBasic: function (photo) {
            this.$store.dispatch('setPhotoBasic', photo);
        },
        deletePhoto: function (photo) {
            this.$store.dispatch('deletePhoto', photo);
        }
    },
    mounted: function () {
        var that = this;

        galleryDropzoneInstance = new Dropzone(this.$refs.dropzone, 
        {
            url: "/Trip/AddPhotos/" + TRIP_ID,
            paramName: 'photos',
            uploadMultiple: true,
            addRemoveLinks: true,
            autoProcessQueue: false,
            acceptedFiles: "image/*",
            parallelUploads: 5,
            maxFiles: 5
        });

        galleryDropzoneInstance.on('successmultiple', function (files, response) {
            if ($.isArray(response)) {
                that.$store.commit('addGalleryPhotos', response);
            }
        })

        galleryDropzoneInstance.on("completemultiple", function(files) {
            for (var i = 0; i < files.length; i++) {
                galleryDropzoneInstance.removeFile(files[i]);
            }
        });
    }
}


var tripPage = new Vue({
    el: "#tripPage",
    store,
    template: "#tripPage-base",
    data: {
        tabs: [{
                title: "путешествие",
                active: true,
                type: "timeline"
            },
            {
                title: "описание",
                active: false,
                type: "description"
            },
            {
                title: "галерея",
                active: false,
                type: "gallery"
            }
        ]
    },
    computed: Vuex.mapState({
        selectedTab: function () {
            for (var i = 0; i < this.tabs.length; i++) {
                if (this.tabs[i].active) return this.tabs[i].type
            }
            return "";
        },
        trip: 'trip'
    }),
    components: {
        "trip-page-heading": tripPageHeading,
        "trip-page-timeline": tripPageTimeline,
        "trip-page-description": tripPageDescription,
        "trip-page-gallery": tripPageGallery,
    },
    methods: {
        addLocation: function () {
            this.$store.commit('setTimelineCreating', true);
            this.$store.dispatch('addDefaultLocation')
        },
        addEvent: function (type) {
            this.$store.commit('setTimelineCreating', true);
            this.$store.dispatch('addDefaultEvent', type)
        },
        viewTypeChanged: function (action) {
            if (action == "toEditMode"){
                this.trip.viewType = "edit"
            }
            if (action == "saveChanges"){
                this.trip.viewType = "overview"
                this.$store.dispatch('saveChanges')
            }
        }
    },
    created: function () {
        this.$store.dispatch("getTripInfo")
    },
    mounted: function () {
        window.sr = ScrollReveal();
        sr.reveal('.timeline-item__block', {
            origin: 'right',
            distance: '100px',
            easing: 'ease-in-out',
            duration: 400,
        });

        setBodyBottomPadding();
    }
})