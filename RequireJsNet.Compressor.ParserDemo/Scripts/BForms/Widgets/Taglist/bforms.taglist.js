var factory = function ($, select2, ui) {


    var Taglist = function (options) {

        this.options = $.extend(true, this.options, options);
    };

    Taglist.prototype.options = {

        alternate: true,
        sortable: false,
        allowDuplicates: true
    };

    // #region init

    Taglist.prototype._init = function () {

        this.$element = this.element;
        this.$select = this.$element.parents('.input-group').find('select');

        this.select2Options = this.$element.data('select2').opts;
        this.currentTagsIndex = '' + 0;

        this._select2PopulateResults = $.proxy(this.select2Options.populateResults, this.$element.data('select2'));

        this.initialTags = $.extend(true, this.options.tags);

        this.options.tags = this._evaluateInitialTags(this.options.tags);

        this.values = [];

        this._setTags(this.options.tags[0]);

        this.stateBeforeRemoval = [];

        this.currentTags = this.options.tags[0];

        this._uid = 0;

        if (this.options.selectedValues) {

            this._testSelectedValues(this.options.selectedValues);

            this._initInitialValues(this.options.selectedValues);

            // this._setSelectedValues(this.options.selectedValues);

            // this.currentTagsIndex = this.options.getNextTags ? this._getIndexFromTags(this.options.getNextTags(this.val(), 0, this.options.tags[0], this.options.tags)) : 0;
        }

        if (this.options.initialTags) {

            this._setInitialTags(this.options.initialTags);
        }

        if (this.options.sortable) {

            this._initSortable();
        }

        this._addDelegates();
    };

    Taglist.prototype._addDelegates = function () {

        this.$element.on('change', $.proxy(this._evSelect2Change, this));
    };

    Taglist.prototype._initSortable = function (killPreviousInstance) {

        var $list = this.$element.parents('.input-group').find('.select2-choices');

        if (killPreviousInstance && $list.data('ui-sortable')) {

            $list.sortable('destroy');
        }

        $list.find('.select2-search-choice').css('cursor', 'pointer');

        $list.css('overflow', 'auto');

        $list.sortable({

            containment: 'parent',
            start: $.proxy(this._evSelect2SortStart, this),
            update: $.proxy(this._evSelect2SortEnd, this)
        });
    };

    Taglist.prototype._setInitialTags = function (initialTags) {

        var newTags = [];

        var ids = initialTags.map(function (value) {

            return value.id || null;
        });

        var textValues = initialTags.map(function (value) {

            return value.text || value;
        });

        for (var idx in initialTags) {

            var id = ids[idx],
                text = textValues[idx],
                tags = this.options.tags[this._getIndexFromTags([{

                    id: id,
                    text: text
                }])];

            if (id == null) {

                for (var i in tags) {

                    if (tags[i].text === text) {

                        id = tags[i].id;
                        break;
                    }
                }
            }

            newTags.push({
                id: id,
                text: text,
                tags: tags,
                _uid: ++this._uid
            });
        }

        this.currentTagsIndex = this._getIndexFromTags(newTags);

        this._setTags(newTags);

        this._fixSelect2Choices();
    };

    Taglist.prototype._initInitialValues = function (selectedValues) {

        this._setSelectedValues(this.options.selectedValues);

        var nextTags = typeof this.options.getNextTags == 'function' ? this.options.getNextTags(this.val(), 0, this.options.tags[0], this.options.tags) : this.options.tags[this.currentTagsIndex];

        this.currentTagsIndex = this._getIndexFromTags(nextTags);

        this.currentTags = this.options.tags[this.currentTagsIndex];
    };

    // #endregion

    // #region private methods

    // #region event handlers

    Taglist.prototype._evSelect2Change = function (e) {

        if (typeof e.added == 'undefined' && typeof e.removed == 'undefined') {

            return;
        }

        var currentTags = this.options.tags[this.currentTagsIndex],
            previousTagsIndex = this.currentTagsIndex,
            nextTags = [];

        if (e.added) {

            var added = e.added;

            this.added = {

                id: added.id || added,
                text: added.text || added,
                tags: currentTags,
                _uid: this._uid
            };
        }

        if (e.removed) {

            var removed = e.removed;

            this.removed = {
                id: removed.id || removed,
                text: removed.text || text,
                tags: currentTags,
                _uid: this._uid
            };
        }

        if (this.options.alternate) {

            var tags = this._getNextTags();

            tags = this._alterTags(tags);

            this._setTags(tags);

            nextTags = tags;
        }

        if (e.added) {

            var added = e.added;

            this.values.push({

                id: added.id || added,
                text: added.text || added,
                tags: currentTags,
                _uid: this._uid
            });

            if (this._isNewTag(e.added, currentTags)) {

                this.options.tags[/*previousTagsIndex*/this.currentTagsIndex].push(e.added);
            }

            var $searchChoice = this.$element.parents('.input-group').find('.select2-choices > .select2-search-choice:last');

            $searchChoice.data('_uid', this._uid);
        }

        if (e.removed) {

            e.preventDefault();

            var removed = $.extend(true, {}, e.removed);

            //removed.id = this._fixId(removed.id || removed);
            //removed.text = removed.text || removed;

            // debugger;

            //for (var i in this.values) {

            //    //if ('' + e.removed.id === '' + this.values[i].id) {

            //    //    this.values.splice(i, 1);
            //    //    break;
            //    //}

            //    if ('' + removed.id === '' + this.values[i].id) {

            //        this.values.splice(i, 1);
            //        break;
            //    }
            //}

            var propperValues = this.$element.select2('val').map($.proxy(function (value) { return this._fixId(value.id || value); }, this)),
                ids = this.values.map(function (value) { return value.id || value; });

            for (var i in ids) {

                if (propperValues[i] !== '' + ids[i]) {

                    this.values.splice(i, 1);
                    break;
                }
            }

            this.removed = e.removed;
        }

        this._uid = this._uid + 1;

        this._fixSelect2Choices();

        if (this.options.sortable && (e.added || e.removed)) {

            this._initSortable();
        }
    };

    Taglist.prototype._evSelect2SortStart = function (e) {

        this.$element.select2('onSortStart');
    };

    Taglist.prototype._evSelect2SortEnd = function (e) {

        this.$element.select2('onSortEnd');

        var values = this._setValuesFromSelect2(this.$element.select2('val'));

        if (this.options.alternate && typeof this.options.getNextTags == 'function') {

            var tags = this.options.getNextTags(values, parseInt(this.currentTagsIndex), this.options.tags[this.currentTagsIndex], this.options.tags);

            this.currentTagsIndex = this._getIndexFromTags(tags);

            tags = this._alterTags(tags);

            this.currentTags = tags;

            this._setTags(tags);

            this._initSortable(true);

            this._fixSelect2Choices();
        }
    };

    // #endregion

    Taglist.prototype._getNextTags = function (nextIndex) {

        var index = nextIndex || (this.currentTagsIndex + 1) % this.options.tags.length,
            tags = [];

        if (typeof this.options.getNextTags == 'function') {

            var values = this.val();

            if (this.added) {

                values.push({

                    id: this.added.id || this.added,
                    text: this.added.text || this.added
                });

                this.added = undefined;
            }

            if (this.removed) {

                var indexToRemove;

                for (var i in values) {

                    if (values[i].id == this.removed.id) {

                        indexToRemove = i;
                        break;
                    }
                }

                if (indexToRemove) {

                    values.splice(indexToRemove, 1);
                }

                this.removed = undefined;
            }

            tags = this.options.getNextTags(values, parseInt(this.currentTagsIndex), this.options.tags[this.currentTagsIndex], this.options.tags);

            index = this._getIndexFromTags(tags);

            this.currentTagsIndex = index;

            this.currentTags = tags.slice();

            return tags;
        }

        this.currentTagsIndex = index;

        tags = this.options.tags[this.currentTagsIndex];

        this.currentTags = tags.slice();

        return tags;
    };

    Taglist.prototype._getIndexFromTags = function (tags) {

        var allTags = this.options.tags;

        for (var index in allTags) {

            var candidateTags = allTags[index].map(function (el) {

                return el.text || el;
            });

            var tagsText = tags.map(function (el) {

                return el.text || el;
            });

            var isMatch = true;

            for (var i in tagsText) {

                if (candidateTags.indexOf(tagsText[i]) == -1) {

                    isMatch = false;
                    break;
                }
            }

            if (isMatch) {

                return index;
            }

        }

        return null;
    };

    Taglist.prototype._setTags = function (tags) {

        tags = $.isArray(tags) ? tags : [];

        var select2Options = {

            tags: tags,
            populateResults: $.proxy(this._populateResults, this),
            matcher: function (query, el, opt) {

                return el.toUpperCase().indexOf(query.toUpperCase()) != -1;
            }
        };

        this.$select.empty();

        for (var i in tags) {

            var tag = tags[i],
                text = tag.text || tag,
                value = tag.id || tag;

            this.$select.append('<option value="' + value + '">' + text + '</option>');
        }

        this.select2Options.tags = [];

        this.$element.select2('destroy');
        this.$element.select2($.extend(true, this.select2Options, select2Options));
    };

    Taglist.prototype._mergeTags = function (tags) {

        var currentTags = this.$element.data('select2').opts.tags;

        for (var i in tags) {

            currentTags.push(tags[i]);
        }

        this._setTags(currentTags);
    };

    Taglist.prototype._populateResults = function ($container, results, query) {

        var tags = this._alterTags(this.currentTags),
            matches = [];

        var finalized = typeof this.options.finalize == 'function' ?
            this.options.finalize(this.val(), this.currentTagsIndex, this.options.tags[this.currentTagsIndex], this.options.tags)
            : false;

        if (finalized) {

            this._select2PopulateResults($container, [], query);

            return;
        }

        for (var i in tags) {

            var text = tags[i].text || tags[i];

            if (query.term == null || query.term === '' || text.indexOf(query.term) != -1) {

                matches.push(tags[i]);
            }
        }

        if (query.term != null && query.term != '') {

            var newTerm = this._createSearchChoice(query.term);

            if (newTerm) {

                matches.push(newTerm);
            }
        }

        if (!this._duplicatesAllowed()) {

            matches = this._removeDuplicates(matches);
        }

        this._select2PopulateResults($container, matches, query);
    };

    Taglist.prototype._alterIds = function (tags) {

        var selectedIds = this.$element.select2('val');

        for (var i in tags) {

            if (selectedIds.indexOf('' + tags[i].id) != -1) {

                tags[i].id = '_' + tags[i].id;
            }
        }

        return tags;
    };

    Taglist.prototype._fixSelect2Choices = function () {

        var $searchChoices = this.$element.parents('.input-group').find('.select2-choices > .select2-search-choice');

        $searchChoices.each($.proxy(function (idx, el) {

            var $choice = $(el),
                $textContainer = $choice.find('div:first');

            for (var i in this.values) {

                if (i === '' + idx) {

                    $textContainer.text(this.values[i].text);
                    break;
                }
            }

        }, this));
    };

    Taglist.prototype._isId = function (text) {

        var currentTags = this.options.tags[this.currentTagsIndex];

        for (var i in currentTags) {

            var id = currentTags[i].id || currentTags[i];

            if ('' + id === text) {

                return true;
            }
        }

        return false;
    };

    Taglist.prototype._getTagById = function (id) {

        var tags = this.options.tags[this.currentTagsIndex];

        for (var i in tags) {

            if (tags[i].id === '' + id) {
                return tags[i];
            }
        }

        return undefined;
    };

    Taglist.prototype._getTagByIdFromAllTags = function (id) {

        var allTags = this.options.tags;

        for (var idx in allTags) {

            var tags = allTags[idx];

            for (var i in tags) {

                if ('' + tags[i].id === '' + id) {

                    return tags[i];
                }
            }
        }

        return undefined;
    };

    Taglist.prototype._getTagByText = function (text) {

        var tags = this.options.tags[this.currentTagsIndex];

        for (var i in tags) {

            if (tags[i].text === text) {
                return tags[i];
            }
        }

        return undefined;
    };

    Taglist.prototype._alterTags = function (currentTags, currentValues) {

        var prefix = '',
            selectedValues = currentValues || this.$element.select2('val'),
            tags = [],
            allowDuplicates = this._duplicatesAllowed();

        for (var i in currentTags) {

            var id = currentTags[i].id || currentTags[i],
                text = currentTags[i].text || currentTags[i];

            if (typeof currentTags[i].id != 'undefined' && allowDuplicates) {

                var count = 0;

                for (var j in selectedValues) {

                    var valueId = selectedValues[j].replace(/_+/g, '');

                    if (valueId === id) {

                        count++;
                    }
                }

                for (var j = 0; j < count; j++) {

                    prefix += '_';
                }
            }

            tags.push({

                id: prefix + id,
                text: text
            });
        }

        return tags;
    };

    Taglist.prototype._fixId = function (id) {

        var matches = id.match(/_+/g);

        if (matches) {

            id = id.slice(id.indexOf(matches[0]) + matches[0].length);
        }

        return id;
    };

    Taglist.prototype._createSearchChoice = function (term) {

        //var finalized = typeof this.options.finalize == 'function' ?
        //    this.options.finalize(this.val(), this.currentTagsIndex, this.options.tags[this.currentTagsIndex], this.options.tags)
        //    : false;

        //if (finalized) {

        //    return null;
        //}

        if (typeof this.options.createSearchChoice != 'undefined') {

            if (typeof this.options.createSearchChoice == 'boolean') {

                return this.options.createSearchChoice ? {

                    id: term,
                    text: term
                } : null;
            }

            if (typeof this.options.createSearchChoice == 'function') {

                return this.options.createSearchChoice(term);
            }
        }

        return true;
    };

    Taglist.prototype._duplicatesAllowed = function () {

        var allowDuplicates = false;

        if (typeof this.options.allowDuplicates != 'undefined') {

            if (typeof this.options.allowDuplicates == 'boolean') {

                allowDuplicates = this.options.allowDuplicates;
            }

            if (typeof this.options.allowDuplicates == 'function') {

                allowDuplicates = this.options.allowDuplicates(this.val(), parseInt(this.currentTagsIndex), this.options.tags[this.currentTagsIndex], this.options.tags);
            }
        }

        return allowDuplicates;
    };

    Taglist.prototype._removeDuplicates = function (tags) {

        var valuesText = this.values.map(function (el) {

            return el.text || el;
        });

        var tagsText = tags.map(function (el) {

            return el.text || el;
        });

        var newValues = [];

        for (var i in tagsText) {

            if (valuesText.indexOf(tagsText[i]) == -1) {

                newValues.push(tags[i]);
            }
        }

        return newValues;
    };

    Taglist.prototype._isNewTag = function (tag, currentTags) {

        var text = tag.text || tag,
            tagsText = currentTags.map(function (el) {

                return el.text || el;
            });

        return tagsText.indexOf(text) == -1;
    };

    Taglist.prototype._evaluateInitialTags = function (tags) {

        // each tags sequence must contain elements defined as such:
        // {
        //    id: [String | Number],
        //    text: [String]
        // }

        // if the initial tags are not given in the above format, they must be altered

        var alteredTags = [];

        for (var idx in tags) {

            var currentSequence = tags[idx],
                alteredSequence = [];

            for (var i in currentSequence) {

                var id = currentSequence[i].id || currentSequence[i],
                    text = currentSequence[i].text || currentSequence[i];

                var item = {

                    id: id,
                    text: text
                };

                alteredSequence.push(item);
            }

            alteredTags.push(alteredSequence);
        }

        return alteredTags;
    };

    Taglist.prototype._setSelectedValues = function (selectedValues) {

        this.values = [];

        var ids = selectedValues.map(function (value) {

            return value.id || null;
        });

        var textValues = selectedValues.map(function (value) {

            return value.text || value;
        });

        for (var idx in selectedValues) {

            var id = ids[idx],
                text = textValues[idx],
                tags = this.options.tags[this._getIndexFromTags([{

                    id: id,
                    text: text
                }])];

            if (id == null) {

                for (var i in tags) {

                    if (tags[i].text === text) {

                        id = tags[i].id;
                        break;
                    }
                }
            }

            this.values.push({
                id: id,
                text: text,
                tags: tags,
                _uid: ++this._uid
            });
        }

        ids = this.values.map(function (value) {

            return value.id;
        });

        var alteredIds = this._alterIds(ids);

        this.$element.select2('val', alteredIds);

        this._fixSelect2Choices();

        return this.values;
    };

    Taglist.prototype._alterIds = function (ids) {

        var alteredIds = [],
            apparitions = {};

        for (var idx in ids) {

            if (typeof apparitions['' + ids[idx]] != 'undefined') {

                apparitions['' + ids[idx]]++;

            } else {

                apparitions['' + ids[idx]] = 1;
            }

            var id = ids[idx];

            if (apparitions['' + ids[idx]] != 1) {

                for (var i = 1; i < apparitions['' + ids[idx]]; i++) {

                    id = '_' + id;
                }
            }

            alteredIds.push(id);

        }

        return alteredIds;
    };

    Taglist.prototype._setSelect2Choices = function (values) {

        var selectedValues = values.map(function (value) {

            return {
                id: value.id || value,
                text: value.text || value
            };
        });

        var $list = this.$element.parents('.input-group').find('.select2-choices');

        $list.empty();

        for (var i in selectedValues) {

            var html = '<li class="select2-search-choice">' +
                           '<div>' + selectedValues[i].text + '</div>' +
                           '<a href="#" onclick="return false;" class="select2-search-choice-close" tabindex="-1"></a>' +
                       '</li>';

            $list.append(html);
        }

        $list.parents('.select2-container').removeClass('select2-active');
    };

    Taglist.prototype._setValuesFromSelect2 = function (values) {

        var ids = values.map($.proxy(this._fixId, this));

        this.values = [];

        for (var i in ids) {

            var id = ids[i],
                tag = this._getTagByIdFromAllTags(id),
                text = tag.text || tag,
                tags = this.options.tags[this._getIndexFromTags([{ id: id, text: text }])];

            this.values.push({
                id: id,
                text: text,
                tags: tags
            });
        }

        return this.values;
    };

    Taglist.prototype._testSelectedValues = function (values) {

        for (var i in values) {

            if (!this._testSelectedValue(values[i])) {

                throw {
                    message: 'No element with text "' + (values[i].text || values[i]) + '" was found in tags'
                };
            }
        }

        return true;
    };

    Taglist.prototype._testSelectedValue = function (value) {

        var allTags = this.options.tags,
            text = value.text || value;

        for (var idx in allTags) {

            var tags = allTags[idx];

            for (var i in tags) {

                if ('' + tags[i].text === '' + text) {

                    return true;
                }
            }
        }

        return false;
    };

    Taglist.prototype._normalizeTags = function (tags) {

        var normalizedTags = tags.map(function (tag) {

            return {
                id: tag.id || tag,
                text: tag.text || tag
            };
        });

        return normalizedTags;
    };

    // #endregion

    // #region public methods

    Taglist.prototype.val = function (tags) {

        if (tags) {

            this._setSelectedValues(tags);
        }

        return this.values.map($.proxy(function (el) {

            return {

                id: typeof el.id != 'undefined' ? this._fixId(el.id) : el,
                text: el.text || el
            };

        }, this));
    };

    Taglist.prototype.valid = function () {

        if (typeof this.options.validate == 'function') {

            return this.options.validate(this.val());
        }

        if (typeof this.options.validate == 'string') {

            var string = this.values.reduce(function (all, current) {

                return (all.text || all) + (current.text || current);
            });

            return new RegExp(this.options.validate, 'g').test(string);
        }

        if (typeof this.options.validate == 'object' && this.options.validate instanceof RegExp) {

            var string = this.values.reduce(function (all, current) {

                return (all.text || all) + (current.text || current);
            });

            return this.options.validate.test(string);
        }

        return true;
    };

    Taglist.prototype.preview = function () {

        var values = this.val();

        if (typeof this.options.preview == 'function') {

            return this.options.preview(values);
        }

        return values.reduce(function (all, current) {

            return (all.text || all) + ', ' + (current.text || current);
        });
    };

    Taglist.prototype.valuesText = function (values) {

        values = values || this.values;

        return values.map(function (value) {

            return value.text || value;
        });
    };

    Taglist.prototype.tags = function (tags, computeNextTags) {

        if (tags && tags instanceof Array) {

            this.options.tags = tags.slice();

            this.currentTagsIndex = '0';

            if (computeNextTags) {

                this.currentTags = this._getNextTags();
            } else {

                this.currentTags = this.options.tags[this.currentTagsIndex];
            }

            this.$element.select2({

                tags: this.currentTags
            });

            this.val([]);

            if (this.options.sortable) {

                this._initSortable(true);
            }
        }

        return this.options.tags;
    };

    // #endregion

    $.widget('bforms.bsTaglist', Taglist.prototype);

    return Taglist;
};

if (typeof window.define == 'function' && define.amd) {

    define('bforms-taglist', ['jquery', 'select2', 'jquery-ui-core'], factory);

} else {

    factory(window.jquery);
}