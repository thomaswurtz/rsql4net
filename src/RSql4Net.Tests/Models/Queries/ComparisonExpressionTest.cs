﻿using System.Collections.Generic;
using System.Text;
using FluentAssertions;

namespace RSql4Net.Tests.Models.Queries
{
    public abstract class ComparisonExpressionTest<T>
    {
        protected abstract T Manifest1();

        protected virtual string Manifest1ToString()
        {
            return Manifest1().ToString();
        }

        protected abstract T Manifest2();

        protected virtual string Manifest2ToString()
        {
            return Manifest2().ToString();
        }
            
        /// <summary>
        /// create string is : ( manifest1, manifest2 )
        /// </summary>
        /// <returns></returns>
        protected virtual string ListManifestToString()
        {
            var builder = new StringBuilder();
            builder.Append("(");
            builder.Append(Manifest1ToString() + ",");
            builder.Append(Manifest2ToString());
            builder.Append(")");
            return builder.ToString();
        }

        /// <summary>
        /// create list of List<T> : ( manifest1, manifest2 ) 
        /// </summary>
        /// <returns></returns>
        protected virtual List<T> ListManifest()
        {
            return new List<T> {Manifest1(), Manifest2()};
        }

        protected MockQuery Actual(object obj, bool nullable = false, MockQuery child = null)
        {
            var property = typeof(MockQuery).GetProperty(obj.GetType().Name + (nullable ? "Null" : "") + "P");
            var result = new MockQuery();
            property?.SetValue(result, obj);
            if (child != null)
            {
                result.ChildP = child;
            }
            return result;
        }
        
        #region =in=

        /// <summary>
        /// test query is :
        ///     - {type}P =in= ( manifest1, manifest2 )
        ///     - childP.{type}P =in= ( manifest1, manifest2 )
        /// </summary>
        /// <param name="closure"></param>
        protected void OnShouldBeIn(char? closure = null)
        {
            var obj = Manifest1();
            var listOfManifest = ListManifestToString();
            
            // {type}P =in= ( manifest1, manifest2 ) 
            var query = $"{Helper.GetJsonPropertyName(obj)}P=in={listOfManifest}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj);
            expected(actual)
                .Should()
                .BeTrue();
            
            // childP.{type}P =in= ( manifest1, manifest2 )
            var child = Actual(obj);
            actual = new MockQuery() { ChildP = child };
            query = $"{Helper.GetChildJsonPropertyName(obj)}P=in={listOfManifest}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should()
                .BeTrue();
        }

        /// <summary>
        /// test query is :
        ///     - {type}NullP =in= ( manifest1, manifest2 )
        ///     - childP.{type}NullP =in= ( manifest1, manifest2 )
        /// </summary>
        protected void OnShouldBeInNullable()
        {
            var obj = Manifest1();
            var listManifest = ListManifestToString();
            
            // {type}NullP =in= ( manifest1, manifest2 ) 
            var query = $"{Helper.GetJsonPropertyName(obj)}NullP=in={listManifest}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj, true);
            expected(actual)
                .Should()
                .BeTrue();
            
            // childP.{type}NullP =in= ( manifest1, manifest2 )
            query = $"{Helper.GetChildJsonPropertyName(obj)}NullP=in={listManifest}";
            expected = Helper.Function<MockQuery>(query);
            actual = new MockQuery() {ChildP = actual};
            expected(actual)
                .Should()
                .BeTrue();
        }
        
        #endregion
        
        #region =out=
      
        /// <summary>
        /// test query is:
        ///     - {type}P =out= ( manifest1, manifest2 )
        ///     - childP.{type}P =out= ( manifest1, manifest2 )
        /// </summary>
        protected void OnShouldBeNotIn()
        {
            var obj = Manifest2();
            var listManifest = ListManifestToString();
            
            // {type}P =out= ( manifest1, manifest2 )
            var query = $"{Helper.GetJsonPropertyName(obj)}P=out={listManifest}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(default(T));
            expected(actual)
                .Should()
                .BeTrue();

            var child = Actual(default(T));
            actual = new MockQuery() {ChildP = child};
            query = $"{Helper.GetChildJsonPropertyName(obj)}P=out={listManifest}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should()
                .BeTrue();
        }

        /// <summary>
        /// test query is :
        ///     - {type}NullP =out= ( manifest1, manifest2 )
        ///     - childP.{type}NullP =out= ( manifest1, manifest2 )
        /// </summary>
        protected void OnShouldBeNotInNullable()
        {
            var obj = Manifest2();
            var listManifest = ListManifestToString();
            var query = $"{Helper.GetJsonPropertyName(obj)}NullP=out={listManifest}";
           
            // {type}NullP =out= ( manifest1, manifest2 ) 
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(default(T), true);
            expected(actual)
                .Should()
                .BeTrue();
            
            // childP.{type}NullP =out= ( manifest1, manifest2 )
            query = $"{Helper.GetChildJsonPropertyName(obj)}NullP=out={listManifest}";
            expected = Helper.Function<MockQuery>(query);
            actual = new MockQuery() {ChildP = actual};
            expected(actual)
                .Should()
                .BeTrue();


        }
        
        #endregion

        #region ==eq== Or ==

        /// <summary>
        /// test query is :
        ///     - {type}P == manifest1
        ///     - {type}P =eq= manifest1
        ///     - {type}P == childP.{type}P
        ///     - {type}P =eq= childP.{type}P 
        /// </summary>
        protected void OnShouldBeEquals()
        {
            var obj = Manifest1();
            var objToString = Manifest1ToString();
            
            // {type}P == manifest1
            var query = $"{Helper.GetJsonPropertyName(obj)}P=={objToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj);
            expected(actual)
                .Should().BeTrue();
           
            // {type}P =eq= manifest1
            query = $"{Helper.GetJsonPropertyName(obj)}P=eq={objToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
            
            //  {type}P=eq=childP.{type}P
            var child = Actual(obj);
            actual = Actual(obj, child: child);
            query = $"{Helper.GetJsonPropertyName(obj)}P=={Helper.GetChildJsonPropertyName(obj)}P";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
            
            //  {type}P=eq=childP.{type}P
            child = Actual(obj);
            actual = Actual(obj, child: child);
            query = $"{Helper.GetJsonPropertyName(obj)}P=eq={Helper.GetChildJsonPropertyName(obj)}P";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
        }
            
        /// <summary>
        /// test query is :
        ///     - {type}NullP == manifest1
        ///     - {type}NullP =eq= manifest1
        ///     - {type}NullP == childP.{type}NullP
        ///     - {type}NullP =eq= childP.{type}NullP
        /// </summary>
        protected void OnShouldBeEqualsWithNullable()
        {
            var obj = Manifest1();
            var objToString = Manifest1ToString();
            
            // {type}NullP == manifest1
            var query = $"{Helper.GetJsonPropertyName(obj)}NullP=={objToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj, true);
            expected(actual)
                .Should().BeTrue();
            
            // {type}NullP =eq= manifest1
            query = $"{Helper.GetJsonPropertyName(obj)}NullP=eq={objToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
            
            //  {type}P=eq=childP.{type}P
            var child = Actual(obj);
            actual = Actual(obj, child: child);
            query = $"{Helper.GetJsonPropertyName(obj)}NullP=={Helper.GetChildJsonPropertyName(obj)}NullP";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
            
            //  {type}P=eq=childP.{type}P
            child = Actual(obj);
            actual = Actual(obj, child: child);
            query = $"{Helper.GetJsonPropertyName(obj)}NullP=eq={Helper.GetChildJsonPropertyName(obj)}NullP";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
        }

        #endregion

        #region =neq= Or !=

        /// <summary>
        /// test query is :
        ///     - {type}P != manifest1
        ///     - {type}P =neq= manifest1
        ///     - {type}P != childP.{type}P
        ///     - {type}P =neq= childP.{type}P
        /// </summary>
        protected void OnShouldBeNotEquals()
        {
            var obj1 = Manifest1();
            var obj2 = Manifest2();
            var obj1ToString = Manifest1ToString();
            
            // {type}P != manifest1
            var query = $"{Helper.GetJsonPropertyName(obj1)}P!={obj1ToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj2);
            expected(actual)
                .Should()
                .BeTrue();
           
            // {type}P =neq= manifest1
            query = $"{Helper.GetJsonPropertyName(obj1)}P=neq={obj1ToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should()
                .BeTrue();
            
            // {type}P != childP.{type}P
            query = $"{Helper.GetJsonPropertyName(obj1)}P!={Helper.GetChildJsonPropertyName(obj1)}";
            actual.ChildP = Actual(obj1);
            expected(actual)
                .Should()
                .BeTrue();
            
            // {type}P =neq= childP.{type}P
            query = $"{Helper.GetJsonPropertyName(obj1)}P=neq={Helper.GetChildJsonPropertyName(obj1)}";
            actual.ChildP = Actual(obj1);
            expected(actual)
                .Should()
                .BeTrue(); 
        }

        /// <summary>
        /// test query is :
        ///     - {type}NullP != manifest1
        ///     - {type}NullP =neq= manifest1
        ///     - {type}NullP != childP.{type}NullP
        ///     - {type}NullP =neq= childP.{type}NullP
        /// </summary>
        protected void OnShouldBeNotEqualsWithNullable()
        {
            var obj1 = Manifest1();
            var obj2 = Manifest2();
            var obj1ToString = Manifest1ToString();
            
            // {type}NullP != manifest1
            var query = $"{Helper.GetJsonPropertyName(obj1)}NullP!={obj1ToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj2, true);
            expected(actual)
                .Should()
                .BeTrue();

            // {type}NullP =eq= manifest1
            query = $"{Helper.GetJsonPropertyName(obj1)}NullP=neq={obj1ToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should()
                .BeTrue();
            
            // {type}NullP != childP.{type}NullP
            query = $"{Helper.GetJsonPropertyName(obj1)}NullP!={Helper.GetChildJsonPropertyName(obj1)}NullP";
            expected = Helper.Function<MockQuery>(query);
            actual.ChildP = Actual(obj1, true);
            expected(actual)
                .Should()
                .BeTrue();
            
            // {type}NullP =neq= childP.{type}NullP
            query = $"{Helper.GetJsonPropertyName(obj1)}NullP=neq={Helper.GetChildJsonPropertyName(obj1)}NullP";
            expected(actual)
                .Should()
                .BeTrue();
        }

        #endregion

        #region =lt= Or <

        /// <summary>
        /// test query is :
        ///     - {type}P < manifest1
        ///     - {type}P =lt= manifest1
        ///     - {type}P < child.{type}P
        ///     - {type}P =lt= child.{type}P
        /// </summary>
        protected void OnShouldBeLowerThan()
        {
            var obj1 = Manifest1();
            var obj2ToString = Manifest2ToString();
            
            // {type}P < manifest1
            var query = $"{Helper.GetJsonPropertyName(obj1)}P<{obj2ToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj1, true);
            expected(actual)
                .Should()
                .BeTrue();

            // {type}P =lt= manifest1
            query = $"{Helper.GetJsonPropertyName(obj1)}P=lt={obj2ToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should()
                .BeTrue();
            
            //  {type}P<childP.{type}P
            var child = Actual(Manifest2(), true);
            actual = Actual(Manifest1(), child: child);
            System.Diagnostics.Debug.Print(Manifest1ToString());
            System.Diagnostics.Debug.Print(Manifest2ToString());
            
            
            /*
            query = $"{Helper.GetJsonPropertyName(obj1)}P<{Helper.GetChildJsonPropertyName(obj1)}P";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should()
                .BeTrue();
            */
            /*
            query = $"{Helper.GetJsonPropertyName(obj1)}P=lt={Helper.GetChildJsonPropertyName(obj1)}P";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should()
                .BeTrue();
                */
        }

        /// <summary>
        /// test query is : {type}NullP < manifest1 OR {type}NullP =lt= manifest1
        /// </summary>
        protected virtual void OnShouldBeLowerThanWithNullable()
        {
            var obj1 = Manifest1();
            var obj2ToString = Manifest2ToString();
            
            // {type}NullP < manifest1
            var query = $"{Helper.GetJsonPropertyName(obj1)}NullP<{obj2ToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj1, true);
            expected(actual)
                .Should().BeTrue();

            // {type}NullP =lt= manifest1
            query = $"{Helper.GetJsonPropertyName(obj1)}NullP=lt={obj2ToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
        }

        #endregion

        #region =le= Or <=
    
        /// <summary>
        /// test query : {type}P <= manifest1 Or {type}P =le= manifest1
        /// </summary>
        protected virtual void OnShouldBeLowerThanOrEquals()
        {
            var obj1 = Manifest1();
            var obj2ToString = Manifest2ToString();
            
            // {type}P <= manifest1
            var query = $"{Helper.GetJsonPropertyName(obj1)}P<={obj2ToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj1, true);
            expected(actual)
                .Should().BeTrue();

            // {type}P =le= manifest1
            query = $"{Helper.GetJsonPropertyName(obj1)}P=le={obj2ToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
        }

        /// <summary>
        /// test query : {type}NullP <= manifest1 Or {type}NullP =le= manifest1
        /// </summary>        
        protected virtual void OnShouldBeLowerThanOrEqualsWithNullable()
        {
            var obj1 = Manifest1();
            var obj2ToString = Manifest2ToString();
            
            // {type}NullP <= manifest1
            var query = $"{Helper.GetJsonPropertyName(obj1)}NullP<={obj2ToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj1, true);
            expected(actual)
                .Should().BeTrue();

            // {type}NullP =le= manifest1
            query = $"{Helper.GetJsonPropertyName(obj1)}NullP=le={obj2ToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
        }

        #endregion

        #region =gt= Or >
        
        /// <summary>
        /// test query : {type}P > manifest1 Or {type}P =gt= manifest1
        /// </summary>
        protected void OnShouldBeGreaterThan()
        {
            var obj2 = Manifest2();
            var obj1ToString = Manifest1ToString();
            
            var query = $"{Helper.GetJsonPropertyName(obj2)}P>{obj1ToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj2);
            expected(actual)
                .Should().BeTrue();

            // {type}NUllP =eq= value
            query = $"{Helper.GetJsonPropertyName(obj2)}P=gt={obj1ToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
        }
        
        /// <summary>
        /// test query : {type}NullP > manifest1 Or {type}NullP =gt= manifest1
        /// </summary>
        protected virtual void OnShouldBeGreaterThanWithNullable()
        {
            var obj2 = Manifest2();
            var obj1ToString = Manifest1ToString();
            
            // {type}NUllP == value
            var query = $"{Helper.GetJsonPropertyName(obj2)}NullP>{obj1ToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj2, true);
            expected(actual)
                .Should().BeTrue();

            // {type}NUllP =eq= value
            query = $"{Helper.GetJsonPropertyName(obj2)}NullP=gt={obj1ToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
        }

        #endregion

        #region =get= Or >=

        /// <summary>
        /// test query : {type}P >= manifest1 Or {type}P =ge= manifest1
        /// </summary>
        protected void OnShouldBeGreaterThanOrEquals()
        {
            var obj2 = Manifest2();
            var obj1ToString = Manifest1ToString();
            
            // {type}P >= value
            var query = $"{Helper.GetJsonPropertyName(obj2)}P>={obj1ToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj2);
            expected(actual)
                .Should().BeTrue();

            // {type}P =ge= value
            query = $"{Helper.GetJsonPropertyName(obj2)}P=ge={obj1ToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
        }

        /// <summary>
        /// test query : {type}NullP >= manifest1 Or {type}NullP =ge= manifest1
        /// </summary>
        protected void OnShouldBeGreaterThanOrEqualsWithNullable()
        {
            var obj2 = Manifest2();
            var obj1ToString = Manifest1ToString();

            // {type}NUllP >= value
            var query = $"{Helper.GetJsonPropertyName(obj2)}NullP>={obj1ToString}";
            var expected = Helper.Function<MockQuery>(query);
            var actual = Actual(obj2, true);
            expected(actual)
                .Should().BeTrue();

            // {type}NullP =ge= value
            query = $"{Helper.GetJsonPropertyName(obj2)}NullP=ge={obj1ToString}";
            expected = Helper.Function<MockQuery>(query);
            expected(actual)
                .Should().BeTrue();
        }

        #endregion

        protected void OnShouldThrowInvalidConversionException(string invalidValue)
        {
            var obj2 = Manifest2();
            var query = $"{Helper.GetJsonPropertyName(obj2)}P=={invalidValue}";
            Helper.Function<MockQuery>(query);
        }
    }
}
