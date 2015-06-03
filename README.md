# LibFree.AspNet.Mvc.Prefetch
A prefetch tag helper that allows to render a list of links rel="prefetch" based on the provided directory path.

### Usage

```html
<prefetch directory="/angularjs_app/views" />
```

The tag helper walks the directory recursively searching for any file and renders a list of ```<link rel="prefetch" href="..." />``` tags.