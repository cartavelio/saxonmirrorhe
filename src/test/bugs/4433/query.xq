xquery version "3.1" encoding "UTF-8";

declare default element namespace "http://www.w3.org/1999/xhtml";

declare namespace atom = "http://www.w3.org/2005/Atom";

declare function local:atom-entry-title($entry as element(atom:entry)) as xs:string {
  $entry/atom:title
};

declare function local:atom-entry-content($entry as element(atom:entry)) as node()* {
  if ($entry/atom:content[@type = 'xhtml']) then
    $entry/atom:content/div/*
  else if ($entry/atom:content[@type = 'html']) then
    error()
  else if ($entry/atom:content[@type = 'text']) then
    $entry/atom:content
  else
    error()
};

<html>
  <head>
    <title>{local:atom-entry-title(./atom:entry)}</title>
  </head>
  <body>
    <article>
        <header>
          <h1>{local:atom-entry-title(./atom:entry)}</h1>
        </header>
        {
          local:atom-entry-content(./atom:entry)
        }
      </article>
  </body>
</html>
