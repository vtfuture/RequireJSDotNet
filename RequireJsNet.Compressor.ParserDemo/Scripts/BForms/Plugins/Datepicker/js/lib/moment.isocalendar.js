(function(factory) {
    if (typeof define === "function" && define.amd) {
        define('moment-calendar', ['moment'], factory);
    } else {
        factory(window.moment);
    }
}(function (moment) {
    var firstIsoWeekOfYear = function(year) {
        var m = moment([year]), day = m.day();

        if (day === 0) {
            day = 7;
        }

        // ISO week is the first week with a Thursday, so if the first
        // day of the year is Friday, move to next Sunday.
        if (day > 4) {
            m.add('weeks', 1);
        }

        m.subtract('days', day - 1);

        return m;
    }

        // 0 => year, 1 => week, 2 => day, 3 => minute
        , index_method_map = ['year', 'week', 'day', 'minute']

        // This method powers the getter/setter methods
        , genericAccessor = function(index, value) {
            if (value) {
                var iso = this.isocalendar();
                iso[index] = value;
                this._d = moment.fromIsocalendar(iso).toDate();
                return this;
            } else {
                return this.isocalendar()[index];
            }
        }

        // Helper method for tying together the getter/setters.
        , accessorFactory = function(index) {
            return function(value) {
                return genericAccessor.call(this, index, value);
            };
        };

    moment.fn.isocalendar = function() {
        var year = this.year(), firstMonday = firstIsoWeekOfYear(year), week = Math.floor(this.diff(firstMonday, 'weeks', true)) + 1, day = this.day(), minute = this.hours() * 60 + this.minutes();

        if (week == 53 && firstIsoWeekOfYear(year + 1) <= this) {
            year += 1;
            week = 1;
        } else if (week < 1) {
            year -= 1;
            week = moment([year, 11, 31, 0, 0]).isocalendar()[1];
        }

        if (day === 0) {
            day = 7;
        }

        return [year, week, day, minute
        ];
    };

    moment.fromIsocalendar = function(array) {
        var date = firstIsoWeekOfYear(array[0]).add({
            weeks: array[1] - 1,
            days: array[2] - 1,
            minutes: array[3]
        });

        return date;
    };

    // isoyear, isomonth, isoday, isominute setters.
    for (var i = 0, _len = index_method_map.length; i < _len; i++) {
        moment.fn['iso' + index_method_map[i]] = accessorFactory(i);
    }
}));